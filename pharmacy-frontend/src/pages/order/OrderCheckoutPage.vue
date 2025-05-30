<template>
  <div class="max-w-7xl mx-auto py-8 px-2">
    <div class="flex items-center gap-3 mb-8">
      <router-link
        to="/cart"
        class="flex items-center text-primary-600 hover:text-primary-700 text-lg group"
      >
        <i class="text-xl fas fa-arrow-left mr-2 group-hover:-translate-x-1 transition-transform duration-150"></i>
      </router-link>
      <h2 class="text-2xl font-bold">Оформление заказа</h2>
    </div>

    <!-- Город + Улица -->
    <div class="mb-6 flex flex-col md:flex-row gap-4 items-end">
      <!-- Город -->
      <div class="w-full md:w-[320px]">
        <label class="block text-base font-medium text-gray-700 mb-2">Выберите город</label>
        <el-select
          v-model="selectedCity"
          filterable
          remote
          reserve-keyword
          placeholder="Введите название города"
          :remote-method="searchCities"
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

      <!-- Улица -->
      <div v-if="selectedCity" class="flex-1">
        <label class="block text-base font-medium text-gray-700 mb-2">Улица</label>
        <el-select
          v-model="selectedStreet"
          filterable
          remote
          reserve-keyword
          placeholder="Введите улицу"
          :remote-method="searchStreets"
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

    <!-- Карта и список аптек -->
    <div v-if="selectedCity" class="mb-8 grid grid-cols-1 md:grid-cols-[320px_1fr] gap-6">
      <!-- Левая колонка -->
      <div class="flex flex-col gap-6">
        <!-- Список аптек -->
        <div class="space-y-3 h-[500px] overflow-y-auto rounded-xl border border-gray-200 p-4 bg-white shadow-sm">
          <div
            v-for="pharmacy in pharmacyList"
            :key="pharmacy.id"
            class="p-3 rounded-lg cursor-pointer border transition duration-200"
            :class="{
              'bg-primary-50 border-primary-600': selectedPharmacy?.id === pharmacy.id,
              'hover:bg-gray-50': selectedPharmacy?.id !== pharmacy.id
            }"
            @click="scrollToAndSelect(pharmacy)"
          >
            <div class="flex items-start gap-2">
              <i class="fas fa-map-marker-alt text-primary-500 mt-1"></i>
              <div>
                <p class="font-semibold text-gray-800">{{ pharmacy.name }}</p>
                <p class="text-sm text-gray-500">{{ pharmacy.openingHours }}</p>
                <p class="text-sm text-gray-500">{{ pharmacy.phone }}</p>
              </div>
            </div>
          </div>
        </div>

        <!-- Способ оплаты -->
        <div class="bg-white border rounded-xl shadow-sm p-4">
          <h3 class="text-base font-semibold text-gray-800 mb-3">Способ оплаты</h3>
          <el-radio-group v-model="paymentMethod" class="flex flex-col gap-3 w-full">
            <el-radio-button
              label="Online"
              class="!w-full !h-12 !text-base !rounded-lg !shadow-sm text-center justify-center"
            >
              💳 Онлайн картой
            </el-radio-button>
            <el-radio-button
              label="OnDelivery"
              class="!w-full !h-12 !text-base !rounded-lg !shadow-sm text-center justify-center"
            >
              📦 При получении
            </el-radio-button>
          </el-radio-group>
        </div>
      </div>

      <!-- Правая колонка -->
      <div class="flex flex-col gap-6">
        <!-- Карта -->
        <div class="h-[500px] rounded-xl overflow-hidden border border-gray-300 shadow-md relative bg-gray-50">
          <MapComponent
            ref="mapComponentRef"
            :city="selectedCity"
            :trigger-initial-load="triggerInitialLoad"
            @select="onPharmacySelect"
            @update:pharmacies="pharmacyList = $event"
            @outside="handleMapOutside"
          />

          <div
            v-if="isOutsideCity"
            class="absolute top-2 right-2 z-[1000] bg-yellow-100 text-yellow-800 border border-yellow-400 px-4 py-2 rounded shadow"
          >
            <p class="text-sm mb-2">Вы вышли за пределы выбранного города.</p>
            <el-button type="warning" size="small" plain @click="returnToCity">Вернуться в город</el-button>
          </div>
        </div>

<!-- Выбранная аптека + кнопка подтверждения -->
<div class="min-h-[178px] p-4 bg-white border rounded-xl shadow-sm flex flex-col justify-between">
  <div>
    <h3 class="text-base font-semibold text-gray-800 mb-2">Выбранная аптека</h3>

<div class="space-y-1 min-h-[48px]">
  <template v-if="selectedPharmacy">
    <p class="text-base font-medium text-gray-900">«{{ selectedPharmacy.name }}»</p>
    <p class="text-sm text-gray-600">{{ selectedPharmacy.address || 'Адрес не найден' }}</p>
  </template>
  <template v-else>
    <p class="text-sm text-gray-400">
      Выберите аптеку из списка или на карте, чтобы продолжить оформление заказа.
    </p>
  </template>
</div>

  </div>

  <div class="text-right pt-2">
    <el-button
      type="primary"
      size="large"
      class="!bg-primary-600 hover:!bg-primary-700 w-full sm:w-auto"
      :disabled="!selectedPharmacy || !paymentMethod"
      @click="submitOrder"
    >
      Подтвердить заказ
    </el-button>
  </div>
</div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import debounce from 'lodash/debounce'
import { ElMessage } from 'element-plus'
import MapComponent from '/src/components/MapComponent.vue'
import { useOrderStore } from '/src/stores/OrderStore'

const router = useRouter()
const mapComponentRef = ref(null)
const orderStore = useOrderStore()

const selectedCity = ref(orderStore.selectedCity)
const selectedStreet = ref(orderStore.selectedStreet)
const selectedPharmacy = ref(orderStore.selectedPharmacy)
const paymentMethod = ref(orderStore.paymentMethod)

const cityOptions = ref([])
const streetOptions = ref([])
const pharmacyList = ref([])
const isOutsideCity = ref(false)
const loadingCities = ref(false)
const loadingStreets = ref(false)
const triggerInitialLoad = ref(false)

watch(selectedCity, val => orderStore.selectedCity = val)
watch(selectedStreet, val => orderStore.selectedStreet = val)
watch(selectedPharmacy, val => orderStore.selectedPharmacy = val)
watch(paymentMethod, val => orderStore.paymentMethod = val)

function handleMapOutside(val) {
  isOutsideCity.value = val
  if (val) selectedPharmacy.value = null
}

function onPharmacySelect(pharmacy) {
  if (selectedPharmacy.value?.id === pharmacy.id) return
  selectedPharmacy.value = pharmacy
}

function onStreetSelect(street) {
  if (street?.lat && street?.lon) {
    mapComponentRef.value?.flyToCoordinates(street.lat, street.lon)
    setTimeout(() => {
      mapComponentRef.value?.fetchInitialPharmacies()
    }, 1000)
  }
}

async function scrollToAndSelect(pharmacy) {
  await mapComponentRef.value?.flyToPharmacy(pharmacy)
}

function submitOrder() {
  if (!selectedPharmacy.value) {
    ElMessage({ message: 'Пожалуйста, выберите аптеку.', type: 'warning' })
    return
  }
  if (!paymentMethod.value) {
    ElMessage({ message: 'Пожалуйста, выберите способ оплаты.', type: 'warning' })
    return
  }

  orderStore.setOrderDetails({
    city: selectedCity.value,
    street: selectedStreet.value,
    pharmacy: selectedPharmacy.value,
    method: paymentMethod.value === 'Online' ? 'Online' : 'OnDelivery'
  })

  router.push({ name: 'OrderSummary' })
}

onMounted(() => {
  if (selectedCity.value) {
    cityOptions.value = [selectedCity.value]
    triggerInitialLoad.value = true
  } else {
    detectCity()
  }
})

function detectCity() {
  const moscow = {
    name: 'Москва',
    display_name: 'Москва',
    lat: 55.7558,
    lng: 37.6173,
    place_id: 'moscow_fallback'
  }

  const setCity = (city) => {
    selectedCity.value = city
    cityOptions.value = [city]
    triggerInitialLoad.value = true
  }

  if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(
      async (pos) => {
        const name = await getCityNameByCoords(pos.coords.latitude, pos.coords.longitude)
        if (name) {
          setCity({
            name,
            display_name: name,
            lat: pos.coords.latitude,
            lng: pos.coords.longitude,
            place_id: `geo_${name}_${pos.coords.latitude}`
          })
        } else {
          setCity(moscow)
        }
      },
      () => setCity(moscow)
    )
  } else {
    setCity(moscow)
  }
}

async function getCityNameByCoords(lat, lon) {
  try {
    const res = await fetch(`https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${lat}&lon=${lon}&addressdetails=1`, {
      headers: { 'User-Agent': 'MediCare-App/1.0 (support@medicare.ru)' }
    })
    const data = await res.json()
    if (data.address?.country_code !== 'ru') return null
    return data.address?.city || data.address?.town || data.address?.village || null
  } catch {
    return null
  }
}

const searchCities = async (query) => {
  if (!query) return
  loadingCities.value = true
  try {
    const res = await fetch(`https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(query)}&format=json&limit=20&addressdetails=1&countrycodes=ru`)
    const data = await res.json()
    const q = query.toLowerCase().trim()
    const unique = new Map()
    for (const place of data) {
      const addr = place.address || {}
      const name = addr.city || addr.town || addr.village
      if (
        place.address?.country_code === 'ru' &&
        name &&
        name.toLowerCase().startsWith(q) &&
        !unique.has(name)
      ) {
        unique.set(name, {
          display_name: name,
          name,
          lat: parseFloat(place.lat),
          lng: parseFloat(place.lon),
          place_id: place.place_id
        })
      }
    }
    cityOptions.value = [...unique.values()]
  } catch {
    cityOptions.value = []
  } finally {
    loadingCities.value = false
  }
}

const searchStreets = debounce(async (query) => {
  if (!query || !selectedCity.value) return
  loadingStreets.value = true
  try {
    const res = await fetch(`https://nominatim.openstreetmap.org/search?street=${encodeURIComponent(query)}&city=${selectedCity.value.name}&format=json&limit=20&addressdetails=1`)
    const data = await res.json()
    const unique = new Map()
    const allowedTypes = ['residential', 'tertiary', 'secondary', 'primary', 'road', 'unclassified', 'service', 'living_street']
    for (const place of data) {
      const address = place.address || {}
      const road = address.road || address.footway || address.pedestrian || address.street
      const type = place.type
      if (road && allowedTypes.includes(type) && !unique.has(road)) {
        unique.set(road, {
          display_name: road,
          place_id: place.place_id,
          lat: parseFloat(place.lat),
          lon: parseFloat(place.lon)
        })
      }
    }
    streetOptions.value = [...unique.values()]
  } catch {
    streetOptions.value = []
  } finally {
    loadingStreets.value = false
  }
}, 300)
</script>
