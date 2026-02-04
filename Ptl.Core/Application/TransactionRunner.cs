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

        // LOAD ONE TRANSACTION (CALLED BY API / CRON)
        public void SeedTestTag(int gateway, int tag, int qty, string txDetailId)
        {
            var tx = new PickTransaction
            {
                TransactionId = "TEST-TX",
                HeaderGetaway = gateway,
                HeaderTag = 99
            };

            tx.ActiveTags.Add(tag);

            _transactions[tx.TransactionId] = tx;
            _tagStates[tag] = new TagState(gateway, tag, qty, txDetailId);
            _tagToTransaction[tag] = tx.TransactionId;
        }

        public void DecreaseTag(TagState state)
        {
            if (state.Quantity <= 0)
                return;

            state.Decrease();

            _display.DisplayQty(
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

        public void StartTransaction(PickTransaction tx)
        {
            // 🔥 MARK AS PROCESSING (status = 1)
            bool ok = _txSource.UpdateTransaction(tx.TransactionId, 1);

            if (!ok)
            {
                _actionSink.EnqueuePendingAction(
                    PendingDbAction.ForTransaction(tx.TransactionId)
                );
                return;
            }

            lock (_sync)
            {
                if (_transactions.ContainsKey(tx.TransactionId))
                    return;
                tx.IsStarted = true;
                _transactions[tx.TransactionId] = tx;
            }

            _display.ShowHeader(
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

        public void CompleteTag(TagState state, PickTransaction tx)
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
                _display.ClearHeader(tx.HeaderGetaway, tx.HeaderTag);
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

        public void RestoreTransaction(PickTransaction tx)
        {
            lock (_sync)
            {
                if (_transactions.ContainsKey(tx.TransactionId))
                    return;

                _transactions[tx.TransactionId] = tx;
            }

            Console.WriteLine($"[RECOVERY] Restoring TX {tx.TransactionId}");

            _display.ShowHeader(
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