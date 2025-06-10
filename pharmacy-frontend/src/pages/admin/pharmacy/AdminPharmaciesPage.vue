<template>
  <div>
    <h1 class="text-2xl font-semibold mb-2">Аптеки</h1>
    <div class="mb-4 text-gray-600">Всего аптек: {{ totalCount }}</div>
    <div class="bg-white rounded-lg shadow p-6 mb-6">
      <el-form :inline="true" @submit.prevent>
        <el-form-item label-width="0">
          <el-input
            v-model="search"
            placeholder="Поиск"
            size="large"
            class="!w-64"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" plain @click="fetch">Поиск</el-button>
          <el-button @click="resetFilters">Сбросить</el-button>
        </el-form-item>
      </el-form>
    </div>
    <div class="flex justify-between mb-4">
      <el-button type="primary" @click="openCreate">
        <i class="fas fa-plus mr-1"></i> Добавить
      </el-button>
      <el-pagination
        layout="sizes, prev, pager, next"
        :total="totalCount"
        :page-size="pageSize"
        :page-sizes="[10, 20, 50]"
        v-model:page-size="pageSize"
        v-model:current-page="pageNumber"
      />
    </div>
    <div class="overflow-x-auto rounded-lg shadow border bg-white">
      <table class="min-w-full table-fixed divide-y divide-gray-200 text-sm">
        <thead
          class="bg-secondary-50 text-left text-secondary-700 uppercase text-sm"
        >
          <tr>
            <th class="px-6 py-5 font-semibold">ID</th>
            <th class="px-6 py-5 font-semibold">Название</th>
            <th class="px-6 py-5 font-semibold">Адрес</th>
            <th class="px-6 py-5 font-semibold">Действия</th>
            <th class="px-6 py-5 font-semibold text-right">
              <span class="sr-only">Детали</span>
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr
            v-for="p in pharmacies"
            :key="p.id"
            class="hover:bg-secondary-50 cursor-pointer"
            @click="goDetails(p.id)"
          >
            <td class="px-6 py-4 whitespace-nowrap">{{ p.id }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ p.name }}</td>
            <td class="px-6 py-4 whitespace-nowrap">
              {{ formatAddress(p.address) }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap" @click.stop>
              <div class="flex gap-2 justify-start">
                <el-button size="small" @click.stop="editPharmacy(p)">
                  <i class="fas fa-edit" />
                </el-button>
                <el-button
                  size="small"
                  type="danger"
                  @click.stop="removePharmacy(p)"
                >
                  <i class="fas fa-trash" />
                </el-button>
              </div>
            </td>
            <td class="px-6 py-4 text-right text-gray-400">
              <i class="fas fa-chevron-right"></i>
            </td>
          </tr>
          <tr v-if="!loading && pharmacies.length === 0">
            <td colspan="5" class="text-center py-6 text-gray-500">
              Аптеки не найдены
            </td>
          </tr>
          <tr v-if="loading">
            <td colspan="5" class="text-center py-6 text-gray-500">
              Загрузка...
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <div class="flex justify-end mt-6">
      <el-pagination
        layout="sizes, prev, pager, next"
        :total="totalCount"
        :page-size="pageSize"
        :page-sizes="[10, 20, 50]"
        v-model:page-size="pageSize"
        v-model:current-page="pageNumber"
      />
    </div>

    <el-dialog
      v-model="dialogVisible"
      width="800px"
      title="Аптека"
      :close-on-click-modal="false"
    >
      <div class="h-96 mb-4">
        <MapComponent
          ref="mapRef"
          :city="selectedCity"
          mode="address"
          @select="onMapSelect"
        />
      </div>
      <el-form label-width="120px" :model="form" :rules="rules" ref="formRef">
        <el-form-item label="Название" prop="name">
          <el-input v-model="form.name" />
        </el-form-item>
        <el-form-item label="Телефон" prop="phone">
          <PhoneInput v-model="form.phone" digits-only />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">Отмена</el-button>
        <el-button type="primary" @click="savePharmacy">Сохранить</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, watch } from "vue";
import { useRouter, useRoute } from "vue-router";
import {
  getPharmacies,
  createPharmacy,
  updatePharmacy,
  deletePharmacy,
} from "/src/services/PharmacyService";
import MapComponent from "/src/components/MapComponent.vue";
import PhoneInput from "/src/components/inputs/PhoneInput.vue";
import { ElMessageBox, ElMessage } from "element-plus";
import { nextTick } from "vue";
import formatAddress from "/src/utils/formatAddress";

const route = useRoute();
const router = useRouter();
const pharmacies = ref([]);
const totalCount = ref(0);
const pageNumber = ref(1);
const pageSize = ref(20);
const loading = ref(false);
const search = ref("");
const dialogVisible = ref(false);
const formRef = ref();
const rules = {
  name: [{ required: true, message: "Введите название", trigger: "blur" }],
};
const editingId = ref(null);
const form = ref({ name: "", phone: "" });
const selectedCity = ref(null);
const mapRef = ref(null);
const newAddress = ref(null);

watch(dialogVisible, async (val) => {
  if (val) {
    await nextTick();
    mapRef.value?.resize();
  }
});

pageNumber.value = Number(route.query.page) || 1;
pageSize.value = Number(route.query.size) || pageSize.value;
search.value = route.query.search || "";

const fetch = async () => {
  loading.value = true;
  try {
    const data = await getPharmacies({
      page: pageNumber.value,
      size: pageSize.value,
      search: search.value,
    });
    pharmacies.value = data.items;
    totalCount.value = data.totalCount;
  } finally {
    loading.value = false;
  }
};

watch(pageNumber, (val) => {
  router.replace({
    query: { ...route.query, page: val, size: pageSize.value },
  });
  fetch();
});

watch(pageSize, (val) => {
  pageNumber.value = 1;
  router.replace({ query: { ...route.query, page: 1, size: val } });
  fetch();
});

const resetFilters = () => {
  search.value = "";
  pageNumber.value = 1;
  fetch();
};

function openCreate() {
  editingId.value = null;
  form.value = { name: "", phone: "" };
  selectedCity.value = null;
  newAddress.value = null;
  dialogVisible.value = true;
}

function editPharmacy(p) {
  editingId.value = p.id;
  form.value = { name: p.name, phone: p.phone || "" };
  selectedCity.value = {
    name: p.address.city || p.address.state,
    display_name: p.address.city || p.address.state,
    lat: p.address.latitude,
    lng: p.address.longitude,
    place_id: `ph_${p.id}`,
  };
  newAddress.value = {
    lat: p.address.latitude,
    lon: p.address.longitude,
    address: {
      city: p.address.city,
      state: p.address.state,
      suburb: p.address.suburb,
      road: p.address.street,
      house_number: p.address.houseNumber,
      postcode: p.address.postcode,
      region: p.address.region,
    },
    osm_id: p.address.osmId,
  };
  dialogVisible.value = true;
  nextTick(() =>
    mapRef.value?.flyToCoordinates(
      p.address.latitude,
      p.address.longitude,
      p.name
    )
  );
}

async function removePharmacy(p) {
  try {
    await ElMessageBox.confirm("Удалить аптеку?", "Подтверждение", {
      confirmButtonText: "Удалить",
      cancelButtonText: "Отмена",
      type: "warning",
    });
  } catch {
    return;
  }
  try {
    await deletePharmacy(p.id);
    ElMessage.success("Удалено");
    fetch();
  } catch {}
}

function onMapSelect(addr) {
  newAddress.value = addr;
}

async function savePharmacy() {
  const valid = await formRef.value.validate().catch(() => false);
  if (!valid || !newAddress.value) return;
  const a = newAddress.value.address || {};
  const payload = {
    name: form.value.name,
    phone: form.value.phone || null,
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
  };
  try {
    if (editingId.value) {
      await updatePharmacy(editingId.value, payload);
      ElMessage.success("Аптека обновлена");
    } else {
      await createPharmacy(payload);
      ElMessage.success("Аптека создана");
    }
    dialogVisible.value = false;
    fetch();
  } catch {}
}

const goDetails = (id) => {
  router.push({ name: "AdminPharmacyDetails", params: { id } });
};

fetch();
</script>
