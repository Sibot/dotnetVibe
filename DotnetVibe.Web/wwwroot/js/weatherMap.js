export async function getCurrentPosition() {
  if (!navigator.geolocation) {
    return null;
  }

  return new Promise((resolve) => {
    navigator.geolocation.getCurrentPosition(
      (position) => resolve({
        latitude: position.coords.latitude,
        longitude: position.coords.longitude
      }),
      () => resolve(null),
      { enableHighAccuracy: true, timeout: 10000 }
    );
  });
}

let mapInstance;
let markerInstance;

function configureDefaultMarkerIcons() {
  if (typeof L === "undefined" || L.Icon.Default._dotnetVibeConfigured) {
    return;
  }

  const imageBase = "/lib/leaflet/dist/images/";
  delete L.Icon.Default.prototype._getIconUrl;
  L.Icon.Default.mergeOptions({
    iconUrl: `${imageBase}marker-icon.png`,
    iconRetinaUrl: `${imageBase}marker-icon-2x.png`,
    shadowUrl: `${imageBase}marker-shadow.png`
  });
  L.Icon.Default._dotnetVibeConfigured = true;
}

function escapeHtml(text) {
  return String(text)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll("\"", "&quot;")
    .replaceAll("'", "&#39;");
}

export function setMapPin(mapElementId, latitude, longitude, label) {
  const element = document.getElementById(mapElementId);
  if (!element || typeof L === "undefined") {
    return;
  }

  configureDefaultMarkerIcons();

  if (!mapInstance) {
    mapInstance = L.map(mapElementId).setView([latitude, longitude], 10);
    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
      attribution: "&copy; OpenStreetMap contributors"
    }).addTo(mapInstance);
  } else {
    mapInstance.setView([latitude, longitude], mapInstance.getZoom());
  }

  if (markerInstance) {
    markerInstance.setLatLng([latitude, longitude]);
  } else {
    markerInstance = L.marker([latitude, longitude]).addTo(mapInstance);
  }

  markerInstance.bindPopup(escapeHtml(label)).openPopup();
  mapInstance.invalidateSize();
}

export function destroyMap() {
  if (markerInstance) {
    markerInstance.remove();
    markerInstance = undefined;
  }

  if (mapInstance) {
    mapInstance.remove();
    mapInstance = undefined;
  }
}
