window.authStorage = {
    set: (key, value) => localStorage.setItem(key, value),
    get: (key) => localStorage.getItem(key),
    remove: (key) => localStorage.removeItem(key)
};

// Funkcja do pobierania pliku (eksport danych RODO)
window.downloadFile = (fileName, content) => {
    const blob = new Blob([content], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
};

// ============ QR CODE FUNCTIONS ============

// Zmienna globalna dla skanera
let html5QrCode = null;

// Uruchom skaner QR
window.startQrScanner = async (elementId, dotNetHelper) => {
    try {
        if (html5QrCode) {
            await html5QrCode.stop();
        }

        html5QrCode = new Html5Qrcode(elementId);

        await html5QrCode.start(
            { facingMode: "environment" }, // tylna kamera
            {
                fps: 10,
                qrbox: { width: 250, height: 250 }
            },
            (decodedText) => {
                // Wywołaj metodę .NET gdy zeskanowano kod
                dotNetHelper.invokeMethodAsync('OnQrScanned', decodedText);
            },
            (errorMessage) => {
                // Ignoruj błędy skanowania (np. brak kodu w kadrze)
            }
        );

        return true;
    } catch (err) {
        console.error("Błąd uruchamiania skanera:", err);
        return false;
    }
};

// Zatrzymaj skaner QR
window.stopQrScanner = async () => {
    try {
        if (html5QrCode) {
            await html5QrCode.stop();
            html5QrCode = null;
        }
    } catch (err) {
        console.error("Błąd zatrzymywania skanera:", err);
    }
};

// Generuj QR code jako obrazek
window.generateQrCode = (elementId, text, size = 200) => {
    try {
        const element = document.getElementById(elementId);
        if (!element) return false;

        // Użyj QRCode z biblioteki 
        element.innerHTML = '';
        new QRCode(element, {
            text: text,
            width: size,
            height: size,
            colorDark: "#000000",
            colorLight: "#ffffff",
            correctLevel: QRCode.CorrectLevel.H
        });

        return true;
    } catch (err) {
        console.error("Błąd generowania QR:", err);
        return false;
    }
};
// ============ WEATHER FUNCTION ============
window.fetchWeather = async (lat, lon) => {
    try {
        const url = `https://api.open-meteo.com/v1/forecast?latitude=${lat}&longitude=${lon}&current=temperature_2m,weather_code&timezone=Europe/Warsaw`;
        const response = await fetch(url);
        if (!response.ok) return null;
        const data = await response.json();
        return data;
    } catch (e) {
        console.error('Weather fetch error:', e);
        return null;
    }
};