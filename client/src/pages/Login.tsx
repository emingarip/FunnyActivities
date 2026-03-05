import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './Login.css';
import { useTranslation } from '../hooks/useTranslation';

const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isLogin, setIsLogin] = useState(true);
  const [formData, setFormData] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });

  const { login, register, isLoading, error, isAuthenticated, clearError } = useAuth();
  const navigate = useNavigate();
  const { t } = useTranslation();

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      navigate('/dashboard');
    }
  }, [isAuthenticated, navigate]);

  // Clear error when switching between login/register
  useEffect(() => {
    clearError();
  }, [isLogin, clearError]);

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await login(email, password);
      navigate('/dashboard');
    } catch (error) {
      // Error is handled by the context
    }
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();

    if (formData.password !== formData.confirmPassword) {
      return;
    }

    try {
      await register({
        email: formData.email,
        password: formData.password,
        firstName: formData.firstName,
        lastName: formData.lastName,
      });
      navigate('/dashboard');
    } catch (error) {
      // Error is handled by the context
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    if (isLogin) {
      if (name === 'email') setEmail(value);
      if (name === 'password') setPassword(value);
    } else {
      setFormData(prev => ({
        ...prev,
        [name]: value
      }));
    }
  };

  if (isAuthenticated) {
    return <div>{t('login_redirecting')}</div>;
  }

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
            <h2>{isLogin ? t('login_title_login') : t('login_title_register')}</h2>
            <p>{isLogin ? t('login_subtitle_login') : t('login_subtitle_register')}</p>
          </div>

          <div className="auth-tabs">
            <button
              className={`tab-button ${isLogin ? 'active' : ''}`}
              onClick={() => setIsLogin(true)}
            >
              {t('login_tab_login')}
            </button>
            <button
              className={`tab-button ${!isLogin ? 'active' : ''}`}
              onClick={() => setIsLogin(false)}
            >
              {t('login_tab_register')}
            </button>
          </div>

          {error && (
            <div className="error-message">
              {error}
            </div>
          )}

          {isLogin ? (
            <form onSubmit={handleLogin} className="login-form">
              <div className="form-group">
                <label htmlFor="email">{t('login_label_email')}</label>
                <input
                  type="email"
                  id="email"
                  name="email"
                  value={email}
                  onChange={handleInputChange}
                  required
                  disabled={isLoading}
                />
              </div>

              <div className="form-group">
                <label htmlFor="password">{t('login_label_password')}</label>
                <input
                  type="password"
                  id="password"
                  name="password"
                  value={password}
                  onChange={handleInputChange}
                  required
                  disabled={isLoading}
                />
              </div>

              <button
                type="submit"
                className="login-button"
                disabled={isLoading}
              >
                {isLoading ? t('login_signing_in') : t('login_sign_in')}
              </button>
            </form>
          ) : (
            <form onSubmit={handleRegister} className="register-form">
              <div className="form-row">
                <div className="form-group">
                  <label htmlFor="firstName">{t('login_label_first_name')}</label>
                  <input
                    type="text"
                    id="firstName"
                    name="firstName"
                    value={formData.firstName}
                    onChange={handleInputChange}
                    required
                    disabled={isLoading}
                  />
                </div>

                <div className="form-group">
                  <label htmlFor="lastName">{t('login_label_last_name')}</label>
                  <input
                    type="text"
                    id="lastName"
                    name="lastName"
                    value={formData.lastName}
                    onChange={handleInputChange}
                    required
                    disabled={isLoading}
                  />
                </div>
              </div>

              <div className="form-group">
                <label htmlFor="registerEmail">{t('login_label_email')}</label>
                <input
                  type="email"
                  id="registerEmail"
                  name="email"
                  value={formData.email}
                  onChange={handleInputChange}
                  required
                  disabled={isLoading}
                />
              </div>

              <div className="form-group">
                <label htmlFor="registerPassword">{t('login_label_password')}</label>
                <input
                  type="password"
                  id="registerPassword"
                  name="password"
                  value={formData.password}
                  onChange={handleInputChange}
                  required
                  disabled={isLoading}
                  minLength={6}
                />
              </div>

              <div className="form-group">
                <label htmlFor="confirmPassword">{t('login_label_confirm_password')}</label>
                <input
                  type="password"
                  id="confirmPassword"
                  name="confirmPassword"
                  value={formData.confirmPassword}
                  onChange={handleInputChange}
                  required
                  disabled={isLoading}
                  minLength={6}
                />
                {formData.password && formData.confirmPassword && formData.password !== formData.confirmPassword && (
                  <span className="password-mismatch">{t('login_passwords_not_match')}</span>
                )}
              </div>

              <button
                type="submit"
                className="register-button"
                disabled={isLoading || formData.password !== formData.confirmPassword}
              >
                {isLoading ? t('login_creating_account') : t('login_create_account')}
              </button>
            </form>
          )}

          <div className="auth-links">
            {isLogin ? (
              <p>
                {t('login_no_account')}{' '}
                <button
                  type="button"
                  className="link-button"
                  onClick={() => setIsLogin(false)}
                >
                  {t('login_sign_up')}
                </button>
              </p>
            ) : (
              <p>
                {t('login_have_account')}{' '}
                <button
                  type="button"
                  className="link-button"
                  onClick={() => setIsLogin(true)}
                >
                  {t('login_sign_in_cta')}
                </button>
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;
