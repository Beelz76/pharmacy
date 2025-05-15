<template>
  <div class="w-full h-full relative">
    <transition name="fade">
      <div
        v-if="isOutsideCity"
        class="absolute top-4 left-1/2 -translate-x-1/2 transform bg-yellow-100 border border-yellow-400 text-yellow-800 px-4 py-2 rounded-md shadow flex items-center gap-3 text-sm z-[1000]"
      >
        <i class="fas fa-triangle-exclamation text-yellow-600 text-lg"></i>
        <span>Вы вышли за пределы выбранного города.</span>
        <el-button type="warning" size="small" plain @click="returnToCity">
          Вернуться
        </el-button>
      </div>
    </transition>

    <l-map
      v-if="mapCenter"
      :zoom="zoom"
      :min-zoom="13"
      :max-zoom="18"
      :center="mapCenter"
      ref="mapRef"
      style="height: 100%; width: 100%"
    >
      <l-tile-layer url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
      <l-marker
        v-for="pharmacy in pharmacies"
        :key="pharmacy.id"
        :lat-lng="[pharmacy.lat, pharmacy.lon]"
        @click="() => selectPharmacy(pharmacy)"
      >
        <l-popup>{{ pharmacy.name }}</l-popup>
      </l-marker>
    </l-map>

    <LoadingSpinner
      v-if="isLoading"
      size="sm"
      color="primary"
      class="absolute top-2 right-2 z-[999]"
    />
  </div>
</template>

<script setup>
import { ref, watch, nextTick, onMounted } from 'vue'
import { LMap, LTileLayer, LMarker, LPopup } from '@vue-leaflet/vue-leaflet'
import 'leaflet/dist/leaflet.css'
import L from 'leaflet'
import * as turf from '@turf/turf'
import LoadingSpinner from './LoadingSpinner.vue'
import { ElButton } from 'element-plus'

// Leaflet icon fix
delete L.Icon.Default.prototype._getIconUrl
L.Icon.Default.mergeOptions({
  iconRetinaUrl: new URL('leaflet/dist/images/marker-icon-2x.png', import.meta.url).href,
  iconUrl: new URL('leaflet/dist/images/marker-icon.png', import.meta.url).href,
  shadowUrl: new URL('leaflet/dist/images/marker-shadow.png', import.meta.url).href,
})

const props = defineProps({
  city: Object,
  triggerInitialLoad: Boolean
})
const emit = defineEmits(['select', 'update:pharmacies'])

const zoom = 13
const mapCenter = ref(null)
const mapRef = ref(null)
const pharmacies = ref([])
const isLoading = ref(false)
const outsideCity = ref(false)
let fetchTimeout = null

function isInsideCity(centerLat, centerLng) {
  const d = turf.distance(
    turf.point([centerLng, centerLat]),
    turf.point([props.city.lng, props.city.lat]),
    { units: 'kilometers' }
  )
  return d <= 15
}

function setupMoveEndListener(map) {
  map.off('moveend')
  map.on('moveend', () => {
    clearTimeout(fetchTimeout)
    fetchTimeout = setTimeout(() => {
      const center = map.getCenter()
      const currentZoom = map.getZoom()
      if (currentZoom >= 13 && isInsideCity(center.lat, center.lng)) {
        outsideCity.value = false
        fetchPharmaciesInBounds(map.getBounds())
      } else {
        outsideCity.value = true
        pharmacies.value = []
        emit('update:pharmacies', [])
      }
    }, 500)
  })
}

onMounted(() => {
  watch(() => mapRef.value?.leafletObject, (map) => {
    if (map) {
      map.whenReady(() => {
        setupMoveEndListener(map)
        fetchPharmaciesInBounds(map.getBounds())
      })
    }
  }, { immediate: true })
})

watch(() => props.city, (city) => {
  if (city?.lat && city?.lng) {
    mapCenter.value = [city.lat, city.lng]
    nextTick(() => {
      const map = mapRef.value?.leafletObject
      if (map) {
        map.setView([city.lat, city.lng], zoom, { animate: true })
      }
    })
  }
}, { immediate: true })

watch(() => props.triggerInitialLoad, async (newVal) => {
  if (!newVal) return
  await nextTick()
  const map = mapRef.value?.leafletObject
  if (!map) return

  map.whenReady(() => {
    const center = map.getCenter()
    const currentZoom = map.getZoom()
    if (currentZoom >= 13 && isInsideCity(center.lat, center.lng)) {
      fetchPharmaciesInBounds(map.getBounds())
      outsideCity.value = false
    }
  })
})

async function fetchPharmaciesInBounds(bounds) {
  const { _southWest, _northEast } = bounds
  const latDiff = Math.abs(_northEast.lat - _southWest.lat)
  const lngDiff = Math.abs(_northEast.lng - _southWest.lng)
  if (latDiff > 1 || lngDiff > 1) return

  const query = `
    [out:json][timeout:10];
    node["amenity"="pharmacy"]["name"](${_southWest.lat},${_southWest.lng},${_northEast.lat},${_northEast.lng});
    out body;
  `
  isLoading.value = true
  try {
    const response = await fetch('https://overpass-api.de/api/interpreter', {
      method: 'POST',
      body: query,
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' }
    })
    const data = await response.json()
    pharmacies.value = data.elements.map(el => {
      const tags = el.tags || {}
      const rawHours = tags.opening_hours || ''
      const formattedHours = formatOpeningHours(rawHours)

      return {
        ...el,
        name: tags.name || 'Аптека',
        openingHours: formattedHours,
        phone: tags['contact:phone'] || '',
      }
    })
    emit('update:pharmacies', pharmacies.value)
  } catch {
    pharmacies.value = []
  } finally {
    isLoading.value = false
  }
}

async function selectPharmacy(pharmacy) {
  let address = ''
  try {
    const url = `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${pharmacy.lat}&lon=${pharmacy.lon}&addressdetails=1`
    const res = await fetch(url, {
      headers: { 'User-Agent': 'MediCare-App/1.0 (support@medicare.ru)' }
    })
    const data = await res.json()
    const addr = data.address || {}

    const addressParts = [
      addr.city || addr.town || addr.village || '',
      addr.road || '',
      addr.house_number || '',
      addr.postcode || ''
    ].filter(Boolean)

    address = addressParts.length > 0 ? addressParts.join(', ') : 'Без адреса'
  } catch {
    address = 'Без адреса'
  }

  emit('select', {
    name: pharmacy.name,
    address,
    lat: pharmacy.lat,
    lon: pharmacy.lon,
    openingHours: pharmacy.openingHours,
    phone: pharmacy.phone,
    id: pharmacy.id
  })
}

function flyToPharmacy(pharmacy) {
  const map = mapRef.value?.leafletObject
  if (map) {
    map.setView([pharmacy.lat, pharmacy.lon], 17, { animate: true })
    selectPharmacy(pharmacy)
  }
}

function flyToCoordinates(lat, lon) {
  const map = mapRef.value?.leafletObject
  if (map) {
    map.setView([lat, lon], 16, { animate: true })
  }
}

function returnToCity() {
  const map = mapRef.value?.leafletObject
  if (map && props.city?.lat && props.city?.lng) {
    map.setView([props.city.lat, props.city.lng], zoom, { animate: true })
    outsideCity.value = false
  }
}

defineExpose({
  selectPharmacy,
  flyToPharmacy,
  flyToCoordinates,
  fetchInitialPharmacies: () => {
    const map = mapRef.value?.leafletObject
    if (map) {
      fetchPharmaciesInBounds(map.getBounds())
      outsideCity.value = false
    }
  }
})

function formatOpeningHours(raw) {
  if (!raw) return ''
  if (raw === '24/7') return 'Круглосуточно'

  return raw
    .replace(/Mo/g, 'Пн')
    .replace(/Tu/g, 'Вт')
    .replace(/We/g, 'Ср')
    .replace(/Th/g, 'Чт')
    .replace(/Fr/g, 'Пт')
    .replace(/Sa/g, 'Сб')
    .replace(/Su/g, 'Вс')
    .replace(/-/g, '–')
    .replace(/;/g, ',')
}
</script>

<style scoped>
.leaflet-container {
  z-index: 0;
}
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
