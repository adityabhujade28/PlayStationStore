import React from 'react';
import { Outlet, Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import styles from './AdminLayout.module.css';

function AdminLayout() {
    const { logout } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();

    const handleLogout = () => {
        logout();
        navigate('/admin/login');
    };

    const menuItems = [
        { label: 'Dashboard', path: '/admin/dashboard' },
        { label: 'Games', path: '/admin/games' },
        { label: 'Users', path: '/admin/users' },
        { label: 'Subscriptions', path: '/admin/subscriptions' },
    ];

    return (
        <div className={styles.container}>
            <aside className={styles.sidebar}>
                <div className={styles.logo}>PS Store Admin</div>
                <nav className={styles.nav}>
                    {menuItems.map((item) => (
                        <Link
                            key={item.path}
                            to={item.path}
                            className={`${styles.navItem} ${location.pathname === item.path ? styles.active : ''}`}
                        >
                            {item.label}
                        </Link>
                    ))}
                </nav>
                <button onClick={handleLogout} className={styles.logoutButton}>
                    Logout
                </button>
            </aside>
            <main className={styles.content}>
                <Outlet />
            </main>
        </div>
    );
}

export default AdminLayout;
