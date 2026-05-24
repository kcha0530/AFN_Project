function Header({ isLoggedIn, onLogout, userName }) {
  const initials = userName ? userName.slice(0, 2).toUpperCase() : "?";

  return (
    <header className="site-header">
      <div className="header-brand">
        <div className="header-logo-icon">✈️</div>
        <div>
          <p className="logo">AFN Project</p>
          <p className="logo-tag">React · ASP.NET Core · .NET Aspire</p>
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
          <a href="#login" className="btn-nav-link">Sign in</a>
        )}
      </nav>
    </header>
  );
}

export default Header;
