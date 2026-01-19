import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const AdminRoute = ({ children }) => {
    const { isAuthenticated, getDecodedToken, loading } = useAuth();
    const location = useLocation();

    if (loading) {
        return <div>Loading...</div>; // Or a proper spinner
    }

    if (!isAuthenticated()) {
        return <Navigate to="/admin/login" state={{ from: location }} replace />;
    }

    const tokenData = getDecodedToken();
    if (tokenData?.role !== 'Admin') {
        return <Navigate to="/admin/login" replace />;
    }

    return children;
};

export default AdminRoute;
