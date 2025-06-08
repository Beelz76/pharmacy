<template>
  <div class="w-full h-full relative">
    <transition name="fade">
      <div
        v-if="mode === 'pharmacy' && isOutsideCity"
        class="absolute top-4 left-1/2 -translate-x-1/2 transform bg-yellow-100 border border-yellow-400 text-yellow-800 px-4 py-2 rounded-md shadow flex items-center gap-3 text-sm z-[1000]"
      >
        <i class="fas fa-triangle-exclamation text-yellow-600 text-lg"></i>
        <span>Вы вышли за пределы выбранного города.</span>
        <el-button type="warning" size="small" plain @click="returnToCity">
          Вернуться
        </el-button>
      </div>
    </transition>

    <div ref="mapContainer" class="w-full h-full"></div>

    <LoadingSpinner
      v-if="isLoading"
      size="sm"
      color="primary"
      class="absolute top-2 right-2 z-[999]"
    />
  </div>
</template>

<script setup>
import { ref, watch, onMounted, nextTick } from "vue";
import maplibregl from "maplibre-gl";
import "maplibre-gl/dist/maplibre-gl.css";
import * as turf from "@turf/turf";
import LoadingSpinner from "./LoadingSpinner.vue";
import { ElButton } from "element-plus";

const props = defineProps({
  city: Object,
  triggerInitialLoad: Boolean,
  mode: { type: String, default: "pharmacy" }, // 'pharmacy' | 'address'
});
const emit = defineEmits(["select", "update:pharmacies", "outside"]);

const zoom = 13;
const mapContainer = ref(null);
let map = null;
const pharmacies = ref([]);
const markers = ref([]);
let marker = null;
const isLoading = ref(false);
const isOutsideCity = ref(false);
let fetchTimeout = null;

function isInsideCity(centerLat, centerLng) {
  const d = turf.distance(
    turf.point([centerLng, centerLat]),
    turf.point([props.city.lng, props.city.lat]),
    { units: "kilometers" }
  );
  return d <= 15;
}

function setupMoveEndListener() {
  if (props.mode !== "pharmacy") return;
  map.on("moveend", () => {
    clearTimeout(fetchTimeout);
    fetchTimeout = setTimeout(() => {
      const center = map.getCenter();
      const currentZoom = map.getZoom();
      if (currentZoom >= 13 && isInsideCity(center.lat, center.lng)) {
        isOutsideCity.value = false;
        emit("outside", false);
        fetchPharmaciesInBounds(map.getBounds());
      } else {
        isOutsideCity.value = true;
        emit("outside", true);
        pharmacies.value = [];
        emit("update:pharmacies", []);
        clearMarkers();
      }
    }, 500);
  });
}

onMounted(() => {
  watch(
    () => props.city,
    (city) => {
      if (city?.lat && city?.lng) {
        if (!map) {
          initMap(city.lat, city.lng);
        } else {
          map.flyTo({ center: [city.lng, city.lat], zoom, essential: true });
        }
      }
    },
    { immediate: true }
  );

  watch(
    () => props.triggerInitialLoad,
    async (newVal) => {
      if (props.mode === "pharmacy" && newVal && map) {
        await nextTick();
        fetchPharmaciesInBounds(map.getBounds());
        isOutsideCity.value = false;
      }
    },
    { immediate: true }
  );
});

function initMap(lat, lng) {
  map = new maplibregl.Map({
    container: mapContainer.value,
    style:
      "https://api.maptiler.com/maps/streets-v2/style.json?key=GzpelqhPkoQ06WlrAxV3",
    center: [lng, lat],
    zoom,
  });

  map.addControl(new maplibregl.NavigationControl());
  if (props.mode === "pharmacy") {
    setupMoveEndListener();
    map.on("load", () => {
      fetchPharmaciesInBounds(map.getBounds());
    });
  } else {
    map.on("click", onAddressClick);
  }
}

function clearMarkers() {
  markers.value.forEach((marker) => marker.remove());
  markers.value = [];
}

function addMarkers(pharmaciesList) {
  clearMarkers();
  pharmaciesList.forEach((pharmacy) => {
    const marker = new maplibregl.Marker({ color: "#3b82f6" })
      .setLngLat([pharmacy.lon, pharmacy.lat])
      .addTo(map);

    marker.getElement().addEventListener("click", () => {
      selectPharmacy(pharmacy);
    });

    markers.value.push(marker);
  });
}

async function fetchPharmaciesInBounds(bounds) {
  const sw = bounds.getSouthWest();
  const ne = bounds.getNorthEast();
  const latDiff = Math.abs(ne.lat - sw.lat);
  const lngDiff = Math.abs(ne.lng - sw.lng);
  if (latDiff > 1 || lngDiff > 1) return;

  const query = `
    [out:json][timeout:10];
    node["amenity"="pharmacy"]["name"](${sw.lat},${sw.lng},${ne.lat},${ne.lng});
    out body;
  `;
  isLoading.value = true;
  try {
    const response = await fetch("https://overpass-api.de/api/interpreter", {
      method: "POST",
      body: query,
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
    });
    const data = await response.json();
    pharmacies.value = data.elements.map((el) => {
      const tags = el.tags || {};
      const rawHours = tags.opening_hours || "";
      const formattedHours = formatOpeningHours(rawHours);

      return {
        ...el,
        name: tags.name || "Аптека",
        openingHours: formattedHours,
        phone: tags["contact:phone"] || "",
      };
    });
    emit("update:pharmacies", pharmacies.value);
    addMarkers(pharmacies.value);
  } catch {
    pharmacies.value = [];
  } finally {
    isLoading.value = false;
  }
}

async function selectPharmacy(pharmacy) {
  let address = "";
  let addr = {};
  let osmId = null;
  try {
    const url = `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${pharmacy.lat}&lon=${pharmacy.lon}&addressdetails=1`;
    const res = await fetch(url, {
      headers: { "User-Agent": "MediCare-App/1.0 (support@medicare.ru)" },
    });
    const data = await res.json();
    addr = data.address || {};
    osmId = data.osm_id || null;

    const addressParts = [
      addr.city || addr.town || addr.village || "",
      addr.road || "",
      addr.house_number || "",
      addr.postcode || "",
    ].filter(Boolean);

    address = addressParts.length > 0 ? addressParts.join(", ") : "Без адреса";
  } catch {
    address = "Без адреса";
  }

  emit("select", {
    name: pharmacy.name,
    address,
    lat: pharmacy.lat,
    lon: pharmacy.lon,
    openingHours: pharmacy.openingHours,
    phone: pharmacy.phone,
    id: pharmacy.id,
    addressData: addr,
    osmId,
  });
}

function flyToPharmacy(pharmacy) {
  if (map) {
    map.flyTo({
      center: [pharmacy.lon, pharmacy.lat],
      zoom: 17,
      essential: true,
    });
    selectPharmacy(pharmacy);
  }
}

function flyToCoordinates(lat, lon) {
  if (map) {
    map.flyTo({ center: [lon, lat], zoom: 16, essential: true });
    if (props.mode === "address") {
      if (marker) marker.remove();
      marker = new maplibregl.Marker({ color: "#3b82f6" })
        .setLngLat([lon, lat])
        .addTo(map);
    }
  }
}

async function onAddressClick(e) {
  if (props.mode !== "address") return;
  const { lat, lng } = e.lngLat;
  if (marker) marker.remove();
  marker = new maplibregl.Marker({ color: "#3b82f6" })
    .setLngLat([lng, lat])
    .addTo(map);

  try {
    const res = await fetch(
      `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${lat}&lon=${lng}&addressdetails=1`,
      { headers: { "User-Agent": "MediCare-App/1.0 (support@medicare.ru)" } }
    );
    const data = await res.json();
    emit("select", {
      display_name: data.display_name,
      lat,
      lon: lng,
      address: data.address || {},
      osm_id: data.osm_id,
    });
  } catch {
    emit("select", { lat, lon: lng });
  }
}

function returnToCity() {
  if (props.mode !== "pharmacy") return;
  if (map && props.city?.lat && props.city?.lng) {
    map.flyTo({
      center: [props.city.lng, props.city.lat],
      zoom,
      essential: true,
    });
    isOutsideCity.value = false;
    emit("outside", false);
  }
}

function formatOpeningHours(raw) {
  if (!raw) return "";
  if (raw === "24/7") return "Круглосуточно";
  return raw
    .replace(/Mo/g, "Пн")
    .replace(/Tu/g, "Вт")
    .replace(/We/g, "Ср")
    .replace(/Th/g, "Чт")
    .replace(/Fr/g, "Пт")
    .replace(/Sa/g, "Сб")
    .replace(/Su/g, "Вс")
    .replace(/-/g, "–")
    .replace(/;/g, ",");
}

defineExpose({
  flyToPharmacy,
  flyToCoordinates,
  fetchInitialPharmacies: () => {
    if (map && props.mode === "pharmacy") {
      fetchPharmaciesInBounds(map.getBounds());
      isOutsideCity.value = false;
    }
  },
});
</script>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
