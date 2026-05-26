function Header({ isLoggedIn, onLogout, userName, onLoginClick }) {
  const initials = userName ? userName.slice(0, 2).toUpperCase() : "";

  return (
    <header className="site-header">
      <div className="header-brand">
        <span className="header-logo-icon">✈</span>
        <div>
          <span className="logo">SkyBook</span>
          <span className="logo-tag">Flight Booking</span>
        </div>
      </div>

      <nav className="header-nav">
        {isLoggedIn ? (
          <>
            <div className="header-user-badge">
              <div className="user-avatar">{initials}</div>
              <span className="user-name">{userName}</span>
            </div>
            <button type="button" className="btn-logout" onClick={onLogout}>
              Sign out
            </button>
          </>
        ) : (
          <button type="button" className="btn-signin" onClick={onLoginClick}>
            Sign in
          </button>
        )}
      </nav>
    </header>
  );
}

export default Header;
