using Ptl.Agent.Domain;
using Ptl.Contracts.Dtos;
using Ptl.Contracts.Events;
using Ptl.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Ptl.Agent.Application
{
    public class TransactionRunner
    {
        private readonly Dictionary<string, PickTransaction> _transactions = new();
        private readonly Dictionary<int, TagState> _tagStates = new();
        private readonly Dictionary<int, string> _tagToTransaction = new();

        private readonly IPickEventStore _eventStore;
        private readonly ITransactionSource _txSource;
        private readonly IPtlDisplay _display;
        private readonly IPtlActionSink _actionSink;
        private readonly ICoreNotifier _notifier;

        private readonly object _sync = new();

        public volatile bool RecoveryCompleted = false;

        public TransactionRunner(
            ITransactionSource txSource,
            IPickEventStore eventStore,
            IPtlDisplay display,
            IPtlActionSink actionSink,
            ICoreNotifier notifier)
        {
            _txSource = txSource;
            _eventStore = eventStore;
            _display = display;
            _actionSink = actionSink;
            _notifier = notifier;
        }

        //public void DecreaseTag(TagState state)
        //{
        //    if (state.Quantity <= 0)
        //        return;

        //    state.Decrease();

        //    _display.DisplayQty(
        //        state.Gateaway,
        //        state.Tag,
        //        state.Quantity
        //    );
        //}
        public async Task DecreaseTag(TagState state)
        {
            if (state.Quantity <= 0)
                return;

            state.Decrease();

            await _display.DisplayQty(
                state.Gateaway,
                state.Tag,
                state.Quantity
            );
        }



        public PickTransaction? GetNextTransaction()
        {
            lock (_sync)
            {
                if (_transactions.Count >= 2)
                    return null;
            }

            var dto = _txSource.GetNextTransaction();
            if (dto == null)
                return null;

            lock (_sync)
            {
                if (IsHeaderTagInUse(dto.HeaderGateaway, dto.HeaderTag))
                {
                    _txSource.UpdateTransaction(dto.TxId, 0);
                    return null;
                }

                if (_transactions.ContainsKey(dto.TxId))
                    return null;

                var tx = new PickTransaction
                {
                    TransactionId = dto.TxId,
                    HeaderTag = dto.HeaderTag,
                    HeaderText = dto.HeaderText,
                    HeaderGetaway = dto.HeaderGateaway
                };

                foreach (var item in dto.DataDetail)
                {
                    int tag = item.Tag;

                    tx.ActiveTags.Add(tag);

                    _tagStates[tag] = new TagState(
                        item.Gateaway,
                        tag,
                        item.Qty,
                        item.TxDetailId
                    );

                    _tagToTransaction[tag] = tx.TransactionId;
                }

                return tx;
            }
        }
        public async Task StartTransaction(PickTransaction tx)
        {
            if (!MarkTransactionProcessing(tx))
                return;

            RegisterTransaction(tx);

            await ShowHeader(tx);
            await ShowActiveTags(tx);
        }

        private bool MarkTransactionProcessing(PickTransaction tx)
        {
            bool ok = _txSource.UpdateTransaction(tx.TransactionId, 1);

            if (!ok)
            {
                _actionSink.EnqueuePendingAction(
                    PendingDbAction.ForTransaction(tx.TransactionId)
                );
                return false;
            }

            return true;
        }

        private void RegisterTransaction(PickTransaction tx)
        {
            lock (_sync)
            {
                if (_transactions.ContainsKey(tx.TransactionId))
                    return;

                tx.IsStarted = true;
                _transactions[tx.TransactionId] = tx;
            }
        }

        private async Task ShowHeader(PickTransaction tx)
        {
            await _display.ShowHeader(
                tx.HeaderGetaway,
                tx.HeaderTag,
                tx.HeaderText
            );
        }

        //private void ShowActiveTags(PickTransaction tx)
        //{
        //    lock (_sync)
        //    {
        //        var readyTags = _display.GetReadyTags(tx.HeaderGetaway);

        //        //if (!readyTags.Contains(tx.HeaderTag))
        //        //{
        //        //    Console.WriteLine(
        //        //        $"[PTL][BLOCK] Header tag {tx.HeaderTag} unavailable on gateway {tx.HeaderGetaway}. Blocking TX {tx.TransactionId}"
        //        //    );

        //        //    // Mark transaction as blocked
        //        //    _txSource.UpdateTransaction(tx.TransactionId, 3);

        //        //    // Optional: notify / enqueue action
        //        //    _actionSink.EnqueuePendingAction(
        //        //        PendingDbAction.ForTransaction(tx.TransactionId)
        //        //    );

        //        //    return;
        //        //}

        //        foreach (var tag in tx.ActiveTags.ToList())
        //        {
        //            //if (!readyTags.Contains(tag))
        //            //{
        //            //    HandleUnavailableTag(tag);
        //            //    continue;
        //            //}

        //            var state = _tagStates[tag];
        //            _display.DisplayQty(state.Gateaway, tag, state.Quantity);
        //        }
        //    }
        //}
        private async Task ShowActiveTags(PickTransaction tx)
        {
            IReadOnlySet<int> readyTags;

            // 1️⃣ Query hardware OUTSIDE lock
            readyTags = await _display.GetReadyTags(tx.HeaderGetaway);

            Console.WriteLine(
                $"[PTL][READY] GW={tx.HeaderGetaway}, Count={readyTags.Count}, Tags={string.Join(",", readyTags.Take(20))}"
            );

            List<TagState> statesToDisplay;

            // 2️⃣ Filter tags INSIDE lock
            lock (_sync)
            {
                statesToDisplay = tx.ActiveTags
                    //.Where(tag => readyTags.Contains(tag))
                    .Select(tag => _tagStates[tag])
                    .ToList();
            }

            // 3️⃣ Send commands OUTSIDE lock
            foreach (var state in statesToDisplay)
            {
                await _display.DisplayQty(
                    state.Gateaway,
                    state.Tag,
                    state.Quantity
                );
            }
        }


        private void HandleUnavailableTag(int tag)
        {
            var state = _tagStates[tag];

            _txSource.MarkDetailUnavailable(state.TxDetailId); // status_picked = 2

            _tagStates.Remove(tag);
            _tagToTransaction.Remove(tag);

            foreach (var tx in _transactions.Values)
            {
                tx.ActiveTags.Remove(tag);
            }
        }

        public void CompleteTransaction(PickTransaction tx)
        {
            bool ok = _txSource.UpdateTransaction(tx.TransactionId, 2);

            if (!ok)
            {
                _actionSink.EnqueuePendingAction(
                    PendingDbAction.ForTransaction(tx.TransactionId)
                );
                return;
            }

            lock (_sync)
            {
                _transactions.Remove(tx.TransactionId);
            }
        }

        public async Task CompleteTag(TagState state, PickTransaction tx)
        {
            if (!tx.IsStarted)
                return;

            var evt = new PickConfirmedEvent
            {
                TxId = tx.TransactionId,
                TxDetailId = state.TxDetailId,
                PickedQty = state.Quantity
            };

            _eventStore.Append(evt);

            // Try fast-path
            if (TryApply(evt))
            {
                _eventStore.MarkProcessed(evt.EventId);
            }

            lock (_sync)
            {
                tx.ActiveTags.Remove(state.Tag);
                _tagStates.Remove(state.Tag);
                _tagToTransaction.Remove(state.Tag);
            }

            if (tx.IsCompleted) { 
                await _display.ClearHeader(tx.HeaderGetaway, tx.HeaderTag);
                lock (_sync)
                {
                    _transactions.Remove(tx.TransactionId);
                }
            }

        }

        public bool TryApply(PickConfirmedEvent evt)
        {
            try
            {
                if (!_txSource.ProcessPicked(evt.TxDetailId, evt.PickedQty))
                    return false;

                _txSource.UpdateTransaction(evt.TxId, 2);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsHeaderTagInUse(int gateway, int tag)
        {
            return _transactions.Values.Any(t =>
                t.HeaderGetaway == gateway &&
                t.HeaderTag == tag
            );
        }

        public bool TryGetTagState(int tag, out TagState state, out PickTransaction tx)
        {
            lock (_sync)
            {
                state = null!;
                tx = null!;

                if (!_tagToTransaction.TryGetValue(tag, out var txId))
                    return false;

                if (!_transactions.TryGetValue(txId, out tx))
                    return false;

                if (!_tagStates.TryGetValue(tag, out state))
                    return false;

                return true;
            }
        }

        public void TryFinalizeTransaction(string txId)
        {
            PickTransaction? tx;

            lock (_sync)
            {
                if (!_transactions.TryGetValue(txId, out tx))
                    return;

                _transactions.Remove(txId);
            }

            _display.ClearHeader(tx.HeaderGetaway, tx.HeaderTag);
        }

        public PickTransaction BuildTransaction(PickTransactionDto dto)
        {
            var tx = new PickTransaction
            {
                TransactionId = dto.TxId,
                HeaderGetaway = dto.HeaderGateaway,
                HeaderTag = dto.HeaderTag,
                HeaderText = dto.HeaderText,
                IsStarted = true
            };

            foreach (var item in dto.DataDetail)
            {
                tx.ActiveTags.Add(item.Tag);

                _tagStates[item.Tag] = new TagState(
                    item.Gateaway,
                    item.Tag,
                    item.Qty,
                    item.TxDetailId
                );

                _tagToTransaction[item.Tag] = tx.TransactionId;
            }

            return tx;
        }

        public async Task RestoreTransaction(PickTransaction tx)
        {
            lock (_sync)
            {
                if (_transactions.ContainsKey(tx.TransactionId))
                    return;

                _transactions[tx.TransactionId] = tx;
            }

            Console.WriteLine($"[RECOVERY] Restoring TX {tx.TransactionId}");

            await _display.ShowHeader(
                tx.HeaderGetaway,
                tx.HeaderTag,
                tx.HeaderText
            );

            lock (_sync)
            {
                foreach (var tag in tx.ActiveTags)
                {
                    var state = _tagStates[tag];
                    _display.DisplayQty(state.Gateaway, tag, state.Quantity);
                }
            }
        }


    }
}