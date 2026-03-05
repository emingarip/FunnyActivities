import React, { useMemo, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';
import { authAPI } from '../services/api';
import { useTranslation } from '../hooks/useTranslation';
import './Login.css';

const ResetPassword: React.FC = () => {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const token = useMemo(() => searchParams.get('token') ?? '', [searchParams]);
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [status, setStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle');
  const [message, setMessage] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!token) {
      setStatus('error');
      setMessage(t('reset_missing_token'));
      return;
    }
    if (password !== confirm) {
      setStatus('error');
      setMessage(t('login_passwords_not_match'));
      return;
    }

    setStatus('loading');
    setMessage('');
    try {
      await authAPI.resetPassword({ token, newPassword: password });
      setStatus('success');
      setMessage(t('reset_success'));
    } catch (err: any) {
      setStatus('error');
      setMessage(err?.message || t('reset_error'));
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
            <h2>{t('reset_title')}</h2>
            <p>{t('reset_subtitle')}</p>
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
              <label htmlFor="password">{t('login_label_password')}</label>
              <input
                type="password"
                id="password"
                name="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                minLength={8}
                disabled={status === 'loading'}
              />
            </div>

            <div className="form-group">
              <label htmlFor="confirm">{t('login_label_confirm_password')}</label>
              <input
                type="password"
                id="confirm"
                name="confirm"
                value={confirm}
                onChange={(e) => setConfirm(e.target.value)}
                required
                minLength={8}
                disabled={status === 'loading'}
              />
              {password && confirm && password !== confirm && (
                <span className="password-mismatch">{t('login_passwords_not_match')}</span>
              )}
            </div>

            <button
              type="submit"
              className="login-button"
              disabled={status === 'loading'}
            >
              {status === 'loading' ? t('reset_submitting') : t('reset_submit')}
            </button>
          </form>

          <div className="auth-links">
            <p>
              {t('reset_back_to_login')}{' '}
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

export default ResetPassword;
