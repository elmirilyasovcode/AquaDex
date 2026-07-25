const API_BASE = '/api/v1';

async function apiRequest(endpoint, options = {}) {
    const response = await fetch(`${API_BASE}${endpoint}`, {
        ...options,
        credentials: 'include', 
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


async function uploadFile(file) {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(`${API_BASE}/upload/image`, {
        method: 'POST',
        credentials: 'include',
        body: formData
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Upload failed (${response.status})`);
    }

    return response.json(); 
}