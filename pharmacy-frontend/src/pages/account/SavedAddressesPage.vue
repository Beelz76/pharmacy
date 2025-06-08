<template>
  <div class="max-w-5xl mx-auto">
    <h2 class="text-2xl font-bold mb-6">Сохраненные адреса</h2>

    <div
      v-if="auth.isAuthenticated"
      class="grid grid-cols-1 md:grid-cols-[1fr] gap-6"
    >
      <!-- Адреса -->
      <div class="space-y-3">
        <div
          v-for="addr in addresses"
          :key="addr.id"
          class="p-4 border rounded-lg bg-white shadow-sm flex justify-between"
        >
          <div>
            <p class="font-medium text-gray-900">{{ addr.fullAddress }}</p>
            <p v-if="addr.comment" class="text-sm text-gray-500">
              {{ addr.comment }}
            </p>
          </div>
          <div class="flex gap-2">
            <el-button size="small" @click="editAddress(addr)">
              <i class="fas fa-edit" />
            </el-button>
            <el-button
              size="small"
              type="danger"
              @click="deleteAddress(addr.id)"
            >
              <i class="fas fa-trash" />
            </el-button>
          </div>
        </div>

        <div
          class="p-4 border-dashed border-2 rounded-lg flex items-center justify-center text-gray-500 cursor-pointer hover:bg-gray-50"
          @click="startAdd"
        >
          <i class="fas fa-plus mr-2" /> Добавить адрес
        </div>
      </div>

      <!-- Диалог редактирования -->
      <el-dialog v-model="editing" width="600px" align-center title="Адрес">
        <div class="h-72 mb-3">
          <MapComponent
            ref="mapRef"
            :city="selectedCity"
            mode="address"
            @select="onMapSelect"
          />
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-3 gap-2 mb-3">
          <el-input v-model="entrance" placeholder="Подъезд" />
          <el-input v-model="floor" placeholder="Этаж" />
          <el-input v-model="apartment" placeholder="Квартира" />
        </div>

        <el-input
          v-model="addressComment"
          placeholder="Комментарий"
          class="mb-3"
        />

        <template #footer>
          <span class="dialog-footer">
            <el-button @click="cancelEdit">Отмена</el-button>
            <el-button type="primary" @click="saveAddress">Сохранить</el-button>
          </span>
        </template>
      </el-dialog>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { ElMessage } from "element-plus";
import MapComponent from "/src/components/MapComponent.vue";
import {
  getUserAddresses,
  createUserAddress,
  updateUserAddress,
  deleteUserAddress,
} from "/src/services/UserAddressService";
import { useAuthStore } from "/src/stores/AuthStore";

const addresses = ref([]);
const selectedCity = ref(null);
const editing = ref(false);
const editingId = ref(null);
const mapRef = ref(null);
const newAddress = ref(null);
const apartment = ref("");
const entrance = ref("");
const floor = ref("");
const addressComment = ref("");
const auth = useAuthStore();

onMounted(() => {
  if (!auth.isAuthenticated) return;
  loadAddresses();
  detectCity();
});

async function loadAddresses() {
  try {
    addresses.value = await getUserAddresses();
  } catch {
    addresses.value = [];
  }
}

function startAdd() {
  editing.value = true;
  editingId.value = null;
  newAddress.value = null;
  apartment.value = "";
  entrance.value = "";
  floor.value = "";
  addressComment.value = "";
}

function editAddress(addr) {
  editing.value = true;
  editingId.value = addr.id;
  newAddress.value = {
    lat: addr.address.latitude,
    lon: addr.address.longitude,
    address: addr.address,
    osm_id: addr.address.osmId,
  };
  apartment.value = addr.apartment || "";
  entrance.value = addr.entrance || "";
  floor.value = addr.floor || "";
  addressComment.value = addr.comment || "";
  mapRef.value?.flyToCoordinates(addr.address.latitude, addr.address.longitude);
}

function cancelEdit() {
  editing.value = false;
}

function onMapSelect(addr) {
  newAddress.value = addr;
}

async function saveAddress() {
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
    if (editingId.value) {
      await updateUserAddress(editingId.value, payload);
    } else {
      await createUserAddress(payload);
    }
    ElMessage.success("Адрес сохранен");
    editing.value = false;
    await loadAddresses();
  } catch {}
}

async function deleteAddress(id) {
  try {
    await deleteUserAddress(id);
    await loadAddresses();
    ElMessage.success("Адрес удален");
  } catch {}
}

function detectCity() {
  const moscow = {
    name: "Москва",
    display_name: "Москва",
    lat: 55.7558,
    lng: 37.6173,
    place_id: "moscow_fallback",
  };

  const setCity = (city) => {
    selectedCity.value = city;
  };

  if (navigator.geolocation) {
    navigator.geolocation.getCurrentPosition(
      async (pos) => {
        try {
          const res = await fetch(
            `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${pos.coords.latitude}&lon=${pos.coords.longitude}&addressdetails=1`
          );
          const data = await res.json();
          if (data.address?.country_code === "ru") {
            setCity({
              name:
                data.address.city || data.address.town || data.address.village,
              display_name: data.display_name,
              lat: pos.coords.latitude,
              lng: pos.coords.longitude,
              place_id: "geo_" + pos.coords.latitude,
            });
            return;
          }
        } catch {}
        setCity(moscow);
      },
      () => setCity(moscow)
    );
  } else {
    setCity(moscow);
  }
}
</script>
