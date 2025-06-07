<template>
  <div ref="mapContainer" class="w-full h-full"></div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import maplibregl from 'maplibre-gl'
import 'maplibre-gl/dist/maplibre-gl.css'

const props = defineProps({
  city: Object
})
const emit = defineEmits(['select'])

const zoom = 13
const mapContainer = ref(null)
let map = null
let marker = null

onMounted(() => {
  watch(
    () => props.city,
    (city) => {
      if (city?.lat && city?.lng) {
        if (!map) {
          initMap(city.lat, city.lng)
        } else {
          map.flyTo({ center: [city.lng, city.lat], zoom })
        }
      }
    },
    { immediate: true }
  )
})

function initMap(lat, lng) {
  map = new maplibregl.Map({
    container: mapContainer.value,
    style: 'https://api.maptiler.com/maps/streets-v2/style.json?key=GzpelqhPkoQ06WlrAxV3',
    center: [lng, lat],
    zoom
  })
  map.addControl(new maplibregl.NavigationControl())
  map.on('click', onClick)
}

async function onClick(e) {
  const { lat, lng } = e.lngLat
  if (marker) marker.remove()
  marker = new maplibregl.Marker({ color: '#3b82f6' })
    .setLngLat([lng, lat])
    .addTo(map)

  try {
    const res = await fetch(
      `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${lat}&lon=${lng}&addressdetails=1`,
      { headers: { 'User-Agent': 'MediCare-App/1.0 (support@medicare.ru)' } }
    )
    const data = await res.json()
    emit('select', {
      display_name: data.display_name,
      lat,
      lon: lng,
      address: data.address || {},
      osm_id: data.osm_id
    })
  } catch {
    emit('select', { lat, lon: lng })
  }
}

function flyToCoordinates(lat, lon) {
  if (map) {
    map.flyTo({ center: [lon, lat], zoom: 16, essential: true })
    if (marker) marker.remove()
    marker = new maplibregl.Marker({ color: '#3b82f6' })
      .setLngLat([lon, lat])
      .addTo(map)
  }
}

defineExpose({ flyToCoordinates })
</script>