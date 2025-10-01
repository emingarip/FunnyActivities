import React, { useState, useEffect } from 'react';
import './Wallet.css';

interface WalletData {
  balance: number;
  currency: string;
  transactions: Transaction[];
}

interface Transaction {
  id: string;
  type: 'income' | 'expense';
  amount: number;
  description: string;
  date: string;
}

const Wallet: React.FC = () => {
  const [walletData, setWalletData] = useState<WalletData | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchWalletData();
  }, []);

  const fetchWalletData = async () => {
    try {
      setLoading(true);
      // Mock data - in real app, this would be API calls
      setWalletData({
        balance: 1250.50,
        currency: 'TRY',
        transactions: [
          {
            id: '1',
            type: 'income',
            amount: 500,
            description: 'Maaş',
            date: '2024-01-15T10:30:00Z'
          },
          {
            id: '2',
            type: 'expense',
            amount: -50,
            description: 'Market Alışverişi',
            date: '2024-01-14T15:20:00Z'
          },
          {
            id: '3',
            type: 'expense',
            amount: -25,
            description: 'Kahve',
            date: '2024-01-13T09:15:00Z'
          }
        ]
      });
    } catch (err) {
      console.error('Failed to load wallet data');
    } finally {
      setLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="page-container">
        <div className="loading">Cüzdan bilgileri yükleniyor...</div>
      </div>
    );
  }

  return (
    <div className="page-container">
      <h1>Cüzdan</h1>

      {walletData && (
        <>
          {/* Balance Card */}
          <div className="balance-card">
            <h2>Toplam Bakiye</h2>
            <div className="balance-amount">
              {walletData.balance.toLocaleString('tr-TR', {
                style: 'currency',
                currency: walletData.currency
              })}
            </div>
          </div>

          {/* Quick Actions */}
          <div className="quick-actions">
            <button className="action-btn">
              <span className="action-icon">💰</span>
              Para Ekle
            </button>
            <button className="action-btn">
              <span className="action-icon">📤</span>
              Gönder
            </button>
            <button className="action-btn">
              <span className="action-icon">📊</span>
              Raporlar
            </button>
          </div>

          {/* Transactions */}
          <div className="transactions-section">
            <h3>Son İşlemler</h3>
            <div className="transactions-list">
              {walletData.transactions.map((transaction) => (
                <div key={transaction.id} className={`transaction-item ${transaction.type}`}>
                  <div className="transaction-info">
                    <span className="transaction-desc">{transaction.description}</span>
                    <span className="transaction-date">
                      {new Date(transaction.date).toLocaleDateString('tr-TR')}
                    </span>
                  </div>
                  <span className={`transaction-amount ${transaction.type}`}>
                    {transaction.amount > 0 ? '+' : ''}{transaction.amount} {walletData.currency}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </>
      )}
    </div>
  );
};

export default Wallet;