import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { authAPI } from '../services/api';
import { useTranslation } from '../hooks/useTranslation';
import './Login.css';

const ForgotPassword: React.FC = () => {
  const { t } = useTranslation();
  const [email, setEmail] = useState('');
  const [status, setStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');
  const [message, setMessage] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setStatus('loading');
    setMessage('');
    try {
      await authAPI.requestPasswordReset({ email });
      setStatus('success');
      setMessage(t('forgot_success'));
    } catch (err: any) {
      setStatus('error');
      setMessage(err?.message || t('forgot_error'));
    }
  };

  const mascotWebp = `${process.env.PUBLIC_URL}/assets/mascot-login.webp`;
  const mascotPng = `${process.env.PUBLIC_URL}/assets/mascot-login.png`;

  return (
    <div className="login-container">
      <div className="login-wrapper">
        <div
          className="mascot-figure"
          aria-hidden="true"
          style={{ backgroundImage: `url(${mascotWebp}), url(${mascotPng})` }}
        />

        <div className="login-card">
          <div className="login-header">
            <h2>{t('forgot_title')}</h2>
            <p>{t('forgot_subtitle')}</p>
          </div>

          {status === 'success' && (
            <div className="info-message success">
              {message}
            </div>
          )}
          {status === 'error' && (
            <div className="error-message">
              {message}
            </div>
          )}

          <form onSubmit={handleSubmit} className="login-form">
            <div className="form-group">
              <label htmlFor="email">{t('login_label_email')}</label>
              <input
                type="email"
                id="email"
                name="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
                disabled={status === 'loading'}
              />
            </div>

            <button
              type="submit"
              className="login-button"
              disabled={status === 'loading'}
            >
              {status === 'loading' ? t('forgot_submitting') : t('forgot_submit')}
            </button>
          </form>

          <div className="auth-links">
            <p>
              {t('forgot_back_to_login')}{' '}
              <Link className="link-button" to="/login">
                {t('login_sign_in_cta')}
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ForgotPassword;
