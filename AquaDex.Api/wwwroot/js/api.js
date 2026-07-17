const API_BASE = '/api';

async function apiRequest(endpoint, options = {}) {
    const response = await fetch(`${API_BASE}${endpoint}`, {
        ...options,
        credentials: 'include', // sends the auth cookie automatically
        headers: {
            'Content-Type': 'application/json',
            ...options.headers
        }
    });

    if (!response.ok) {
        const errorBody = await response.text();
        let errorMessage;
        try {
            const parsed = JSON.parse(errorBody);
            errorMessage = Array.isArray(parsed) ? parsed.join(', ') : (parsed.title || errorBody);
        } catch {
            errorMessage = errorBody || `Request failed (${response.status})`;
        }
        throw new Error(errorMessage);
    }

    const contentType = response.headers.get('content-type');
    if (contentType && contentType.includes('application/json')) {
        return response.json();
    }
    return null;
}