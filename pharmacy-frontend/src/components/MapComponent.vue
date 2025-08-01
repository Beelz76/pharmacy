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

    <div class="absolute top-2 left-2 z-[999] flex gap-2">
      <div class="flex flex-col w-64">
        <el-select
          v-model="selectedCity"
          filterable
          remote
          reserve-keyword
          placeholder="Город"
          :remote-method="loadCities"
          :loading="loadingCities"
          class="w-full"
        >
          <el-option
            v-for="item in cityOptions"
            :key="item.place_id"
            :label="item.display_name"
            :value="item"
          />
        </el-select>
      </div>

      <div v-if="selectedCity" class="flex flex-col w-64">
        <el-select
          v-model="selectedStreet"
          filterable
          remote
          reserve-keyword
          placeholder="Улица"
          :remote-method="loadStreets"
          :loading="loadingStreets"
          class="w-full"
          @change="onStreetSelect"
        >
          <el-option
            v-for="item in streetOptions"
            :key="item.place_id"
            :label="item.display_name"
            :value="item"
          />
        </el-select>
      </div>
    </div>

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
import debounce from "lodash/debounce";

const defaultCity = {
  name: "Москва",
  display_name: "Москва",
  lat: 55.7558,
  lng: 37.6173,
  place_id: "moscow_default",
  boundingbox: null,
};

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

const selectedCity = ref(props.city || defaultCity);
const selectedStreet = ref(null);
const cityOptions = ref([]);
const streetOptions = ref([]);
const loadingCities = ref(false);
const loadingStreets = ref(false);

function isInsideCity(bounds) {
  const city = selectedCity.value;
  if (!city) return false;
  if (city.boundingbox?.length === 4) {
    const [south, north, west, east] = city.boundingbox.map((v) =>
      parseFloat(v)
    );
    return !(
      bounds.getNorth() < south ||
      bounds.getSouth() > north ||
      bounds.getEast() < west ||
      bounds.getWest() > east
    );
  }
  const center = bounds.getCenter();
  const d = turf.distance(
    turf.point([center.lng, center.lat]),
    turf.point([city.lng, city.lat]),
    { units: "kilometers" }
  );
  return d <= 10;
}

function setupMoveEndListener() {
  if (props.mode !== "pharmacy") return;
  map.on("moveend", () => {
    clearTimeout(fetchTimeout);
    fetchTimeout = setTimeout(() => {
      const currentZoom = map.getZoom();
      const bounds = map.getBounds();
      if (isInsideCity(bounds)) {
        isOutsideCity.value = false;
        emit("outside", false);
        if (currentZoom >= 13) {
          fetchPharmaciesInBounds(bounds);
        } else {
          pharmacies.value = [];
          emit("update:pharmacies", []);
          clearMarkers();
        }
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
      selectedCity.value = city || defaultCity;
    },
    { immediate: true }
  );

  watch(
    selectedCity,
    (city) => {
      emit("update:city", city);
      selectedStreet.value = null;
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
      flyToPharmacy(pharmacy);
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

function flyToCoordinates(lat, lon, label) {
  if (map) {
    map.flyTo({ center: [lon, lat], zoom: 16, essential: true });
    if (props.mode === "address") {
      if (marker) marker.remove();
      marker = new maplibregl.Marker({ color: "#3b82f6" })
        .setLngLat([lon, lat])
        .setPopup(
          new maplibregl.Popup({ offset: 25 }).setText(
            label || "Выбранный адрес"
          )
        )
        .addTo(map);
      marker.togglePopup();
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
    marker;
    const addr = data.address || {};
    const short = [
      addr.city || addr.town || addr.village,
      addr.road,
      addr.house_number,
      addr.postcode,
    ]
      .filter(Boolean)
      .join(", ");

    marker
      .setPopup(
        new maplibregl.Popup({ offset: 25 }).setText(short || "Адрес не найден")
      )
      .togglePopup();
    emit("select", {
      display_name: data.display_name,
      lat,
      lon: lng,
      address: data.address || {},
      osm_id: data.osm_id,
    });
  } catch {
    marker
      .setPopup(new maplibregl.Popup({ offset: 25 }).setText("Выбранный адрес"))
      .togglePopup();
    emit("select", { lat, lon: lng });
  }
}

function returnToCity() {
  if (props.mode !== "pharmacy") return;
  const city = selectedCity.value;
  if (map && city?.lat && city?.lng) {
    map.flyTo({
      center: [city.lng, city.lat],
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

async function searchCities(query) {
  if (!query) return [];
  try {
    const res = await fetch(
      `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(
        query
      )}&format=json&limit=20&addressdetails=1&countrycodes=ru`
    );
    const data = await res.json();
    const q = query.toLowerCase().trim();
    const unique = new Map();
    for (const place of data) {
      const addr = place.address || {};
      const name = addr.city || addr.town || addr.village;
      if (
        place.address?.country_code === "ru" &&
        name &&
        name.toLowerCase().startsWith(q) &&
        !unique.has(name)
      ) {
        unique.set(name, {
          display_name: name,
          name,
          lat: parseFloat(place.lat),
          lng: parseFloat(place.lon),
          place_id: place.place_id,
          boundingbox: place.boundingbox
            ? place.boundingbox.map((v) => parseFloat(v))
            : null,
        });
      }
    }
    return [...unique.values()];
  } catch {
    return [];
  }
}

const loadCities = async (query) => {
  if (!query) return;
  loadingCities.value = true;
  try {
    cityOptions.value = await searchCities(query);
  } finally {
    loadingCities.value = false;
  }
};

const loadStreets = debounce(async (query) => {
  if (!query || !selectedCity.value) return;
  loadingStreets.value = true;
  try {
    streetOptions.value = await searchStreets(query, selectedCity.value.name);
  } finally {
    loadingStreets.value = false;
  }
}, 300);

function onStreetSelect(street) {
  if (street?.lat && street?.lon) {
    flyToCoordinates(street.lat, street.lon);
    setTimeout(() => {
      fetchPharmaciesInBounds(map.getBounds());
    }, 1000);
  }
}

async function searchStreets(query, cityName) {
  if (!query || !cityName) return [];
  try {
    const res = await fetch(
      `https://nominatim.openstreetmap.org/search?street=${encodeURIComponent(
        query
      )}&city=${cityName}&format=json&limit=20&addressdetails=1`
    );
    const data = await res.json();
    const unique = new Map();
    const allowedTypes = [
      "residential",
      "tertiary",
      "secondary",
      "primary",
      "road",
      "unclassified",
      "service",
      "living_street",
    ];
    for (const place of data) {
      const address = place.address || {};
      const road =
        address.road || address.footway || address.pedestrian || address.street;
      const type = place.type;
      if (road && allowedTypes.includes(type) && !unique.has(road)) {
        unique.set(road, {
          display_name: road,
          place_id: place.place_id,
          lat: parseFloat(place.lat),
          lon: parseFloat(place.lon),
        });
      }
    }
    return [...unique.values()];
  } catch {
    return [];
  }
}

defineExpose({
  flyToPharmacy,
  flyToCoordinates,
  searchCities,
  searchStreets,
  loadCities,
  loadStreets,
  resize: () => {
    if (map) map.resize();
  },
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
