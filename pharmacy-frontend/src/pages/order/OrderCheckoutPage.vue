<template>
  <div class="max-w-7xl mx-auto py-8 px-2">
    <!-- Заголовок -->
    <div class="flex items-center gap-3 mb-6">
      <router-link
        to="/cart"
        class="flex items-center text-primary-600 hover:text-primary-700 text-lg group"
      >
        <i
          class="text-xl fas fa-arrow-left mr-2 group-hover:-translate-x-1 transition-transform duration-150"
        ></i>
      </router-link>
      <h2 class="text-2xl font-bold">Оформление заказа</h2>
    </div>

    <!-- Переключатели: способ получения и способ оплаты -->
    <div
      class="mb-6 flex flex-col md:flex-row md:items-center md:justify-between gap-4"
    >
      <el-radio-group v-model="isDelivery" class="flex gap-4">
        <el-radio-button :label="false">Самовывоз</el-radio-button>
        <el-radio-button :label="true">Доставка</el-radio-button>
      </el-radio-group>

      <el-radio-group v-model="paymentMethod" class="flex gap-4">
        <el-radio-button label="Online">Онлайн картой</el-radio-button>
        <el-radio-button label="OnDelivery">При получении</el-radio-button>
      </el-radio-group>
    </div>

    <!-- Самовывоз -->
    <div
      v-if="selectedCity && !isDelivery"
      class="mb-8 grid grid-cols-1 md:grid-cols-[320px_1fr] gap-6"
    >
      <!-- Список аптек и способ оплаты -->
      <div class="flex flex-col gap-6">
        <div
          class="space-y-3 h-[500px] overflow-y-auto rounded-xl border border-gray-200 p-4 bg-white shadow-sm"
        >
          <p class="text-base font-semibold mb-2">Выберите аптеку</p>
          <div
            v-for="pharmacy in pharmacyList"
            :key="pharmacy.id"
            class="p-3 rounded-lg cursor-pointer border transition duration-200"
            :class="{
              'bg-primary-50 border-primary-600':
                selectedPharmacy?.id === pharmacy.id,
              'hover:bg-gray-50': selectedPharmacy?.id !== pharmacy.id,
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
      </div>

      <!-- Карта и подтверждение -->
      <div class="flex flex-col gap-6">
        <div
          class="h-[500px] rounded-xl overflow-hidden border border-gray-300 shadow-md relative bg-gray-50"
        >
          <MapComponent
            ref="mapComponentRef"
            :city="selectedCity"
            :trigger-initial-load="triggerInitialLoad"
            @select="onPharmacySelect"
            @update:pharmacies="pharmacyList = $event"
            @outside="handleMapOutside"
          />
        </div>

        <div class="p-4 bg-white border rounded-xl shadow-sm">
          <h3 class="text-base font-semibold text-gray-800 mb-2">
            Выбранная аптека
          </h3>
          <div class="min-h-[48px] mb-2">
            <template v-if="selectedPharmacy">
              <p class="text-base font-medium text-gray-900">
                {{ selectedPharmacy.name }}
              </p>
              <p class="text-sm text-gray-600">
                {{ selectedPharmacy.address || "Адрес не найден" }}
              </p>
            </template>
            <template v-else>
              <p class="text-sm text-gray-400">
                Выберите аптеку из списка или на карте.
              </p>
            </template>
          </div>
          <el-button
            type="primary"
            size="large"
            class="w-full sm:w-auto"
            :disabled="!selectedPharmacy || !paymentMethod"
            @click="submitOrder"
            >Подтвердить заказ</el-button
          >
        </div>
      </div>
    </div>

    <!-- Доставка -->
    <div
      v-if="selectedCity && isDelivery"
      class="mb-8 grid grid-cols-1 md:grid-cols-[320px_1fr] gap-6"
    >
      <!-- Список адресов и оплата -->
      <div class="flex flex-col gap-6">
        <div
          class="space-y-3 h-[500px] overflow-y-auto rounded-xl border border-gray-200 p-4 bg-white shadow-sm"
        >
          <p class="text-base font-semibold mb-2">Выберите сохранённый адрес</p>
          <div
            v-for="addr in addresses"
            :key="addr.id"
            class="p-3 rounded-lg cursor-pointer border transition"
            :class="{
              'bg-primary-50 border-primary-600': selectedAddressId === addr.id,
              'hover:bg-gray-50': selectedAddressId !== addr.id,
            }"
            @click="selectSavedAddress(addr)"
          >
            <p class="font-semibold text-gray-800">{{ addr.fullAddress }}</p>
          </div>
        </div>

        <div
          class="text-right bg-white border rounded-xl shadow-sm p-4"
          v-if="newAddress"
        >
          <p class="text-sm text-left mb-3">
            {{ getShortAddressFromRaw(newAddress) }}
          </p>
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-2 mb-2">
            <el-input v-model="entrance" placeholder="Подъезд" />
            <el-input v-model="floor" placeholder="Этаж" />
            <el-input v-model="apartment" placeholder="Квартира" />
          </div>
          <el-input
            v-model="addressComment"
            placeholder="Комментарий"
            class="mb-2"
          />
          <el-button type="primary" size="small" @click="saveNewAddress"
            >Сохранить адрес</el-button
          >
        </div>

        <div class="bg-white border rounded-xl shadow-sm p-4" v-else>
          <p class="text-sm text-gray-500">
            Кликните на карту, чтобы выбрать новый адрес.
          </p>
        </div>
      </div>

      <!-- Карта и подтверждение -->
      <div class="flex flex-col gap-6">
        <div
          class="h-[500px] rounded-xl overflow-hidden border border-gray-300 shadow-md relative bg-gray-50"
        >
          <MapComponent
            mode="address"
            ref="addressMapRef"
            :city="selectedCity"
            @select="onMapAddressSelect"
            @update:city="selectedCity = $event"
          />
        </div>

        <div class="p-4 bg-white border rounded-xl shadow-sm">
          <h3 class="text-base font-semibold text-gray-800 mb-2">
            Выбранный адрес
          </h3>
          <div class="min-h-[48px]">
            <template v-if="selectedAddressId">
              <p class="text-sm text-gray-600">
                {{ selectedAddress?.fullAddress }}
              </p>
            </template>
            <template v-else>
              <p class="text-sm text-gray-400">
                Выберите адрес из списка или на карте.
              </p>
            </template>
          </div>
          <el-input
            v-model="deliveryComment"
            type="textarea"
            rows="2"
            placeholder="Комментарий к доставке"
            class="mt-3"
          />
          <el-button
            type="primary"
            size="large"
            class="mt-4 w-full sm:w-auto"
            :disabled="(!selectedAddressId && !newAddress) || !paymentMethod"
            @click="submitOrder"
            >Подтвердить заказ</el-button
          >
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import { useRouter } from "vue-router";
import { ElMessage } from "element-plus";
import MapComponent from "/src/components/MapComponent.vue";
import { useOrderStore } from "/src/stores/OrderStore";
import {
  getUserAddresses,
  createUserAddress,
} from "/src/services/UserAddressService";

const router = useRouter();
const mapComponentRef = ref(null);
const addressMapRef = ref(null);
const orderStore = useOrderStore();

const selectedCity = ref(orderStore.selectedCity);
const selectedStreet = ref(orderStore.selectedStreet);
const selectedPharmacy = ref(orderStore.selectedPharmacy);
const selectedAddressId = ref(orderStore.selectedAddressId);
const selectedAddress = ref(orderStore.selectedAddress);
const isDelivery = ref(orderStore.isDelivery);
const paymentMethod = ref(orderStore.paymentMethod);

const pharmacyList = ref([]);
const addresses = ref([]);
const newAddress = ref(null);
const apartment = ref("");
const entrance = ref("");
const floor = ref("");
const addressComment = ref("");
const deliveryComment = ref(orderStore.deliveryComment || "");
const isOutsideCity = ref(false);
const triggerInitialLoad = ref(false);

watch(selectedCity, (val) => (orderStore.selectedCity = val));
watch(selectedStreet, (val) => (orderStore.selectedStreet = val));
watch(selectedPharmacy, (val) => (orderStore.selectedPharmacy = val));
watch(selectedAddressId, (val) => (orderStore.selectedAddressId = val));
watch(selectedAddress, (val) => (orderStore.selectedAddress = val));
watch(isDelivery, (val) => (orderStore.isDelivery = val));
watch(paymentMethod, (val) => (orderStore.paymentMethod = val));
watch(deliveryComment, (val) => (orderStore.deliveryComment = val));

function handleMapOutside(val) {
  isOutsideCity.value = val;
  if (val) selectedPharmacy.value = null;
}

function onPharmacySelect(pharmacy) {
  if (selectedPharmacy.value?.id === pharmacy.id) return;
  selectedPharmacy.value = pharmacy;
}

async function scrollToAndSelect(pharmacy) {
  await mapComponentRef.value?.flyToPharmacy(pharmacy);
}

async function loadAddresses() {
  try {
    addresses.value = await getUserAddresses();
  } catch {
    addresses.value = [];
  }
}

function selectSavedAddress(addr) {
  selectedAddressId.value = addr.id;
  selectedAddress.value = addr;
  newAddress.value = null;
  selectedCity.value = {
    name:
      addr.address.city ||
      addr.address.town ||
      addr.address.village ||
      "Москва",
    display_name:
      addr.address.city ||
      addr.address.town ||
      addr.address.village ||
      "Москва",
    lat: addr.address.latitude,
    lng: addr.address.longitude,
    place_id: `addr_${addr.id}`,
    boundingbox: null,
  };
  addressMapRef.value?.flyToCoordinates(
    addr.address.latitude,
    addr.address.longitude,
    addr.fullAddress
  );
}

function onMapAddressSelect(addr) {
  newAddress.value = addr;
  selectedAddressId.value = null;
  selectedAddress.value = null;
}

async function saveNewAddress() {
  if (!newAddress.value) return;
  const a = newAddress.value.address || {};
  const payload = {
    address: {
      osmId: newAddress.value.osm_id?.toString() || null,
      region: a.region || a.state || null,
      state: a.state || null,
      city: a.city || a.town || a.village || null,
      suburb: a.suburb || null,
      street: a.road || null,
      houseNumber: a.house_number || null,
      postcode: a.postcode || null,
      latitude: parseFloat(newAddress.value.lat),
      longitude: parseFloat(newAddress.value.lon),
    },
    apartment: apartment.value || null,
    entrance: entrance.value || null,
    floor: floor.value || null,
    comment: addressComment.value || null,
  };
  try {
    const res = await createUserAddress(payload);
    selectedAddressId.value = res.id;
    await loadAddresses();
    selectedAddress.value = addresses.value.find((a) => a.id === res.id);
    newAddress.value = null;
    apartment.value = "";
    entrance.value = "";
    floor.value = "";
    addressComment.value = "";
    ElMessage.success("Адрес сохранен");
  } catch {}
}

function submitOrder() {
  if (isDelivery.value) {
    if (!selectedAddressId.value && !newAddress.value) {
      ElMessage({
        message: "Выберите или сохраните адрес доставки.",
        type: "warning",
      });
      return;
    }
  } else if (!selectedPharmacy.value) {
    ElMessage({ message: "Пожалуйста, выберите аптеку.", type: "warning" });
    return;
  }

  if (!paymentMethod.value) {
    ElMessage({
      message: "Пожалуйста, выберите способ оплаты.",
      type: "warning",
    });
    return;
  }

  orderStore.setOrderDetails({
    city: selectedCity.value,
    street: selectedStreet.value,
    pharmacy: selectedPharmacy.value,
    address: selectedAddress.value,
    addressId: selectedAddressId.value,
    isDelivery: isDelivery.value,
    address: selectedAddress.value,
    addressId: selectedAddressId.value,
    isDelivery: isDelivery.value,
    method: paymentMethod.value === "Online" ? "Online" : "OnDelivery",
    deliveryComment: deliveryComment.value,
  });

  router.push({ name: "OrderSummary" });
}

onMounted(() => {
  if (selectedCity.value) {
    triggerInitialLoad.value = true;
  } else {
    detectCity();
  }
  loadAddresses();
});

function detectCity() {
  const moscow = {
    name: "Москва",
    display_name: "Москва",
    lat: 55.7558,
    lng: 37.6173,
    place_id: "moscow_fallback",
    boundingbox: null,
  };

  const setCity = (city) => {
    selectedCity.value = city;
    triggerInitialLoad.value = true;
  };

  if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(
      async (pos) => {
        const name = await getCityNameByCoords(
          pos.coords.latitude,
          pos.coords.longitude
        );
        if (name) {
          setCity({
            name,
            display_name: name,
            lat: pos.coords.latitude,
            lng: pos.coords.longitude,
            place_id: `geo_${name}_${pos.coords.latitude}`,
            boundingbox: null,
          });
        } else {
          setCity(moscow);
        }
      },
      () => setCity(moscow)
    );
  } else {
    setCity(moscow);
  }
}

async function getCityNameByCoords(lat, lon) {
  try {
    const res = await fetch(
      `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${lat}&lon=${lon}&addressdetails=1`,
      {
        headers: { "User-Agent": "MediCare-App/1.0 (support@medicare.ru)" },
      }
    );
    const data = await res.json();
    if (data.address?.country_code !== "ru") return null;
    return (
      data.address?.city || data.address?.town || data.address?.village || null
    );
  } catch {
    return null;
  }
}

function getShortAddressFromRaw(addr) {
  if (!addr?.address) return "";
  const a = addr.address;
  const parts = [
    a.city || a.town || a.village,
    a.road,
    a.house_number,
    a.postcode,
  ].filter(Boolean);
  return parts.join(", ");
}

const searchCities = async (query) => {
  if (!query) return;
  loadingCities.value = true;
  try {
    cityOptions.value =
      (await mapComponentRef.value?.searchCities(query)) || [];
  } catch {
    cityOptions.value = [];
  } finally {
    loadingCities.value = false;
  }
};
</script>
