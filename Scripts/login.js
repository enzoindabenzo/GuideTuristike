function clearSelectiveCookies() {
    const cookiesToPreserve = [
        '.AspNet.ExternalCookie',
        '.AspNet.ApplicationCookie',
        '.AspNet.TwoFactorCookie',
        '.AspNet.TwoFactorRememberBrowser',
        '__RequestVerificationToken',
        'OpenIdConnect.nonce',
        'OpenIdConnect.state',
        'MSPAuth', 'MSPProf', 'MSPOK', 'MSPRequ', 'MSCC', 'wlidperf',
        'ESTSAUTH', 'ESTSAUTHPERSISTENT', 'ESTSAUTHLIGHT', 'SignInStateCookie', 'buid', 'x-ms-gateway-slice'
    ];

    document.cookie.split(';').forEach(function (cookie) {
        const name = cookie.split('=')[0].trim();
        if (!cookiesToPreserve.some(preserve => name.includes(preserve))) {
            document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/';
            document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=' + window.location.pathname;
            document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/;domain=' + window.location.hostname;
        }
    });

    if (typeof Storage !== "undefined") {
        const currentPath = window.location.pathname.toLowerCase();
        const isOAuthCallback =
            currentPath.includes('/signin-') ||
            currentPath.includes('/account/externallogincallback') ||
            window.location.search.includes('code=') ||
            window.location.search.includes('state=') ||
            window.location.search.includes('session_state=');

        if (!isOAuthCallback) {
            const oauthKeys = [];
            for (let i = 0; i < sessionStorage.length; i++) {
                const key = sessionStorage.key(i);
                if (key && (key.includes('oauth') || key.includes('msal') || key.includes('oidc'))) {
                    oauthKeys.push(key);
                }
            }

            for (let i = sessionStorage.length - 1; i >= 0; i--) {
                const key = sessionStorage.key(i);
                if (key && !oauthKeys.includes(key)) {
                    sessionStorage.removeItem(key);
                }
            }
        }
    }
}

// Initialize Leaflet Map Function
function initializeLeafletMap() {
    // Check if we're on the ManageLocations page
    const mapContainer = document.getElementById('map');
    if (!mapContainer) {
        return; // Exit if no map container found
    }

    console.log("Initializing Leaflet map...");

    // Check if Leaflet is loaded
    if (typeof L === 'undefined') {
        console.error('Leaflet library is not loaded!');
        return;
    }

    try {
        // Initialize map for Albania
        var map = L.map('map').setView([41.3275, 19.8187], 7);
        console.log("Map initialized successfully");

        // Try multiple tile sources due to CSP restrictions
        var tileLayer;

        // First try OpenStreetMap
        try {
            tileLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; OpenStreetMap contributors',
                maxZoom: 19
            });
            tileLayer.addTo(map);
            console.log("OpenStreetMap tiles loaded");
        } catch (error) {
            console.log("OpenStreetMap failed, trying alternative:", error);

            // Fallback to CartoDB tiles (often more CSP-friendly)
            try {
                tileLayer = L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
                    attribution: '&copy; OpenStreetMap &copy; CartoDB',
                    subdomains: 'abcd',
                    maxZoom: 19
                });
                tileLayer.addTo(map);
                console.log("CartoDB tiles loaded");
            } catch (error2) {
                console.log("CartoDB failed, trying Stamen:", error2);

                // Another fallback
                try {
                    tileLayer = L.tileLayer('https://stamen-tiles-{s}.a.ssl.fastly.net/terrain/{z}/{x}/{y}.png', {
                        attribution: 'Map tiles by Stamen Design, CC BY 3.0 &mdash; Map data &copy; OpenStreetMap',
                        subdomains: 'abcd',
                        maxZoom: 18
                    });
                    tileLayer.addTo(map);
                    console.log("Stamen tiles loaded");
                } catch (error3) {
                    console.error("All tile sources failed:", error3);

                    // Show error message to user
                    const mapContainer = document.getElementById('map');
                    if (mapContainer) {
                        const errorDiv = document.createElement('div');
                        errorDiv.style.cssText = 'position: absolute; top: 10px; left: 10px; background: rgba(255,255,255,0.9); padding: 10px; border-radius: 5px; z-index: 1000; color: red; font-weight: bold;';
                        errorDiv.innerHTML = 'Map tiles blocked by security policy. Click will still work for coordinates.';
                        mapContainer.style.position = 'relative';
                        mapContainer.appendChild(errorDiv);
                    }
                }
            }
        }
        console.log("Tile layer added");

        var marker = L.marker([41.3275, 19.8187], { draggable: true }).addTo(map);
        console.log("Marker added");

        // Update coordinates when marker is moved
        function updateCoordinates(latlng) {
            const latInput = document.getElementById('Latitude');
            const lngInput = document.getElementById('Longitude');
            if (latInput && lngInput) {
                latInput.value = latlng.lat.toFixed(6);
                lngInput.value = latlng.lng.toFixed(6);
            }
        }

        // Initial coordinates
        updateCoordinates(marker.getLatLng());

        marker.on('dragend', function (e) {
            updateCoordinates(e.target.getLatLng());
        });

        map.on('click', function (e) {
            marker.setLatLng(e.latlng);
            updateCoordinates(e.latlng);
        });

        // Invalidate size to ensure proper rendering
        setTimeout(function () {
            map.invalidateSize();
            console.log("Map size invalidated");
        }, 250);

    } catch (error) {
        console.error("Error initializing map:", error);
    }
}

// Form validation for location form
function initializeLocationFormValidation() {
    const addLocationForm = document.getElementById('addLocationForm');
    if (!addLocationForm) {
        return; // Exit if no form found
    }

    addLocationForm.addEventListener('submit', function (e) {
        const latInput = document.getElementById('Latitude');
        const lngInput = document.getElementById('Longitude');

        if (!latInput || !lngInput) {
            return;
        }

        let lat = parseFloat(latInput.value);
        let lng = parseFloat(lngInput.value);

        if (isNaN(lat) || lat < 39 || lat > 43) {
            alert("Latitude duhet të jetë midis 39° dhe 43° për Shqipërinë");
            e.preventDefault();
            return false;
        }

        if (isNaN(lng) || lng < 19 || lng > 21.5) {
            alert("Longitude duhet të jetë midis 19° dhe 21.5° për Shqipërinë");
            e.preventDefault();
            return false;
        }
    });
}

// Run on page navigation
if (window.performance && window.performance.navigation.type === window.performance.navigation.TYPE_NAVIGATE) {
    const currentPath = window.location.pathname.toLowerCase();
    const isOAuthCallback =
        currentPath.includes('/signin-') ||
        currentPath.includes('/account/externallogincallback') ||
        window.location.search.includes('code=') ||
        window.location.search.includes('state=') ||
        window.location.search.includes('session_state=') ||
        window.location.search.includes('error=') ||
        window.location.hash.includes('access_token=') ||
        window.location.hash.includes('id_token=');

    if (!isOAuthCallback) {
        clearSelectiveCookies();
    }
}

// Main DOMContentLoaded event handler
document.addEventListener('DOMContentLoaded', function () {
    // Initialize Leaflet map if map container exists
    initializeLeafletMap();

    // Initialize location form validation
    initializeLocationFormValidation();

    // External login form handling
    var externalLoginForm = document.getElementById('externalLoginForm');
    if (externalLoginForm) {
        externalLoginForm.addEventListener('submit', function (e) {
            const provider = e.submitter ? e.submitter.value : '';
            console.log('External login form submitting with provider: ' + provider);
            sessionStorage.setItem('oauthProvider', provider);

            try {
                const form = e.target;
                let action = form.action;
                if (action && !action.includes('prompt=')) {
                    const separator = action.includes('?') ? '&' : '?';
                    action += separator + 'prompt=select_account';
                    form.action = action;
                }
            } catch (err) {
                console.log('Could not modify form action:', err);
            }

            // Provider-specific logic
            if (provider.toLowerCase() === 'facebook') {
                try {
                    if (typeof FB !== 'undefined' && FB.getLoginStatus) {
                        FB.logout();
                    }
                } catch (err) {
                    console.log('Facebook SDK not available or already logged out');
                }
            } else if (provider.toLowerCase().includes('microsoft') || provider.toLowerCase().includes('azure')) {
                console.log('Starting Microsoft authentication flow');
                sessionStorage.setItem('skipCookieClear', 'true');

                const isGuestMode = !window.navigator.storage || !window.indexedDB ||
                    (navigator.webdriver === undefined && !window.chrome?.runtime);

                const timeout = isGuestMode ? 120000 : 30000;

                setTimeout(function () {
                    sessionStorage.removeItem('skipCookieClear');
                }, timeout);
            }

            setTimeout(function () {
                sessionStorage.removeItem('skipCookieClear');
            }, 30000);
        });
    }

    // OAuth error handling
    if (window.location.search.includes('error=')) {
        const urlParams = new URLSearchParams(window.location.search);
        const error = urlParams.get('error');
        const errorDescription = urlParams.get('error_description');

        console.log('OAuth Error:', error);
        if (errorDescription) {
            console.log('Error Description:', decodeURIComponent(errorDescription));
        }

        if (window.location.search.includes('azure_failed')) {
            console.log('Azure AD authentication failed');
        } else if (error === 'access_denied') {
            console.log('OAuth access denied by user');
        } else if (error === 'invalid_request') {
            console.log('Invalid OAuth request');
        } else if (error === 'temporarily_unavailable') {
            console.log('Microsoft authentication temporarily unavailable - security block detected');
            const errorMsg = document.createElement('div');
            errorMsg.className = 'alert alert-warning';
            errorMsg.innerHTML = `
                <strong>Security Notice:</strong> Microsoft has temporarily blocked this sign-in attempt for security reasons. 
                <br>Please try:
                <ul>
                    <li>Using a different device or network</li>
                    <li>Clearing your browser cache and cookies</li>
                    <li>Using a VPN if available</li>
                    <li>Trying again later</li>
                </ul>
            `;
            document.body.insertBefore(errorMsg, document.body.firstChild);
        } else {
            console.log('OAuth authentication error occurred:', error);
        }
    }

    if (window.location.search.includes('code=') && window.location.search.includes('state=')) {
        console.log('OAuth callback received, processing authentication...');
    }
});