import React, { useState, useEffect } from 'react';
import './Wallet.css';
import { useTranslation } from '../hooks/useTranslation';

type TransactionType = 'income' | 'expense';

interface WalletData {
  balance: number;
  currency: string;
  transactions: Transaction[];
}

interface Transaction {
  id: string;
  type: TransactionType;
  amount: number;
  description: string;
  date: string;
}

const Wallet: React.FC = () => {
  const [walletData, setWalletData] = useState<WalletData | null>(null);
  const [loading, setLoading] = useState(true);
  const { t, locale } = useTranslation();

  useEffect(() => {
    fetchWalletData();
  }, []);

  const fetchWalletData = async () => {
    try {
      setLoading(true);
      setWalletData({
        balance: 1250.5,
        currency: 'TRY',
        transactions: [
          {
            id: '1',
            type: 'income',
            amount: 500,
            description: t('wallet_tx_salary'),
            date: '2024-01-15T10:30:00Z',
          },
          {
            id: '2',
            type: 'expense',
            amount: -50,
            description: t('wallet_tx_groceries'),
            date: '2024-01-14T15:20:00Z',
          },
          {
            id: '3',
            type: 'expense',
            amount: -25,
            description: t('wallet_tx_coffee'),
            date: '2024-01-13T09:15:00Z',
          },
        ],
      });
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="page-container">
        <div className="loading">{t('wallet_loading')}</div>
      </div>
    );
  }

  if (!walletData) return null;

  return (
    <div className="page-container">
      <h1>{t('wallet_title')}</h1>

      <div className="balance-card">
        <h2>{t('wallet_total_balance')}</h2>
        <div className="balance-amount">
          {walletData.balance.toLocaleString(locale === 'tr' ? 'tr-TR' : 'en-US', {
            style: 'currency',
            currency: walletData.currency,
          })}
        </div>
      </div>

      <div className="quick-actions">
        <button className="action-btn">
          <span className="action-icon">💰</span>
          {t('wallet_action_add')}
        </button>
        <button className="action-btn">
          <span className="action-icon">📤</span>
          {t('wallet_action_send')}
        </button>
        <button className="action-btn">
          <span className="action-icon">📊</span>
          {t('wallet_action_reports')}
        </button>
      </div>

      <div className="transactions-section">
        <h3>{t('wallet_recent_transactions')}</h3>
        <div className="transactions-list">
          {walletData.transactions.map((transaction) => (
            <div key={transaction.id} className={`transaction-item ${transaction.type}`}>
              <div className="transaction-info">
                <span className="transaction-desc">{transaction.description}</span>
                <span className="transaction-date">
                  {new Date(transaction.date).toLocaleDateString(locale === 'tr' ? 'tr-TR' : 'en-US')}
                </span>
              </div>
              <span className={`transaction-amount ${transaction.type}`}>
                {transaction.amount > 0 ? '+' : ''}
                {transaction.amount} {walletData.currency}
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default Wallet;
