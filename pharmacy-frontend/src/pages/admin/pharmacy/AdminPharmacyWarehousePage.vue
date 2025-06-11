<template>
  <div>
    <div class="flex items-center gap-4 mb-8">
      <button
        @click="router.back()"
        class="flex items-center text-primary-600 hover:text-primary-700 text-base group transition"
      >
        <i
          class="text-xl fas fa-arrow-left mr-2 group-hover:-translate-x-1 duration-150"
        />
      </button>
      <h2 class="text-2xl font-bold tracking-tight">
        Склад аптеки {{ pharmacy?.name || "#" + pharmacy?.id }}
      </h2>
    </div>

    <div v-if="loading" class="text-center py-10">Загрузка...</div>

    <div v-else>
      <div class="flex justify-start mb-4">
        <div></div>
        <el-button type="primary" @click="openAdd">
          <i class="fas fa-plus mr-1"></i> Добавить товар
        </el-button>
      </div>

      <div class="overflow-x-auto rounded-lg shadow border bg-white">
        <table class="min-w-full table-fixed divide-y divide-gray-200 text-sm">
          <thead
            class="bg-secondary-50 text-left text-secondary-700 uppercase text-sm"
          >
            <tr>
              <th class="px-6 py-5 font-semibold">ID</th>
              <th class="px-6 py-5 font-semibold">Название</th>
              <th class="px-6 py-5 font-semibold">Количество</th>
              <th class="px-6 py-5 font-semibold">Цена</th>
              <th class="px-6 py-5 font-semibold">Доступен</th>
              <th class="px-6 py-5 font-semibold">Действия</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100">
            <tr v-for="p in products" :key="p.productId">
              <td class="px-6 py-4">{{ p.productId }}</td>
              <td class="px-6 py-4">
                <RouterLink
                  :to="{
                    name: 'AdminProductDetails',
                    params: { id: p.productId },
                    query: route.query,
                  }"
                  class="text-primary-600 hover:underline"
                >
                  {{ p.productName }}
                </RouterLink>
              </td>
              <td class="px-6 py-4">{{ p.stockQuantity }}</td>
              <td class="px-6 py-4">
                {{ p.price
                }}<span v-if="p.isGlobalPrice" class="text-xs text-gray-500">
                  (глобальная)</span
                >
              </td>
              <td class="px-6 py-4">{{ p.isAvailable ? "Да" : "Нет" }}</td>
              <td class="px-6 py-4">
                <div class="flex justify-start gap-2">
                  <el-button size="small" @click="goProduct(p.productId)">
                    <i class="fas fa-link" />
                  </el-button>
                  <el-button size="small" @click="editProduct(p)">
                    <i class="fas fa-edit" />
                  </el-button>
                  <el-button
                    size="small"
                    type="danger"
                    @click="removeProduct(p.productId)"
                  >
                    <i class="fas fa-trash" />
                  </el-button>
                </div>
              </td>
            </tr>
            <tr v-if="products.length === 0">
              <td colspan="6" class="text-center py-6 text-gray-500">
                Нет данных
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <el-dialog
      v-model="dialogVisible"
      title="Товар"
      width="500px"
      :close-on-click-modal="false"
    >
      <el-form label-width="120px" :model="form" :rules="rules" ref="formRef">
        <el-form-item label="Товар" prop="productId" v-if="!editingId">
          <div class="flex gap-2">
            <el-input
              v-model.number="form.productId"
              type="number"
              min="1"
              class="!w-28"
            />
            <el-autocomplete
              v-model="form.productName"
              :fetch-suggestions="fetchProductSuggestions"
              @select="onProductSelect"
              class="flex-1"
            />
          </div>
        </el-form-item>
        <el-form-item label="Количество" prop="stockQuantity">
          <el-input v-model.number="form.stockQuantity" type="number" min="0" />
        </el-form-item>
        <el-form-item label="Цена" prop="price">
          <el-input v-model.number="form.price" type="number" min="0" />
        </el-form-item>
        <el-form-item label="Доступен" :required="false">
          <el-switch v-model="form.isAvailable" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="dialogVisible = false">Отмена</el-button>
        <el-button type="primary" @click="saveProduct">Сохранить</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  getPharmacyById,
  getPharmacyProducts,
  addPharmacyProduct,
  updatePharmacyProduct,
  deletePharmacyProduct,
} from "/src/services/PharmacyService";
import { ElMessageBox, ElMessage } from "element-plus";
import api from "/src/utils/axios";
import ProductService from "/src/services/ProductService";

const route = useRoute();
const router = useRouter();
const pharmacyId = Number(route.params.id);
const pharmacy = ref(null);
const products = ref([]);
const loading = ref(false);

const dialogVisible = ref(false);
const editingId = ref(null);
const form = ref({
  productId: null,
  productName: "",
  stockQuantity: 0,
  price: null,
  isAvailable: true,
});
const formRef = ref();
const rules = {
  productId: [{ required: true, message: "ID обязателен", trigger: "blur" }],
  stockQuantity: [
    { required: true, message: "Введите количество", trigger: "blur" },
  ],
  price: [
    { min: 0, message: "Цена не может быть отрицательной", trigger: "blur" },
  ],
};

onMounted(async () => {
  loading.value = true;
  try {
    pharmacy.value = await getPharmacyById(pharmacyId);
    products.value = await getPharmacyProducts(pharmacyId);
  } finally {
    loading.value = false;
  }
});

watch(
  () => form.value.productId,
  async (val) => {
    if (!val) {
      form.value.productName = "";
      return;
    }
    try {
      const p = await ProductService.getById(val);
      form.value.productName = p.name;
    } catch {}
  }
);

const fetchProductSuggestions = async (query, cb) => {
  const trimmed = query.trim();
  if (!trimmed || trimmed.length < 2) {
    cb([]);
    return;
  }
  try {
    const res = await api.get(
      `/products/search-suggestions?query=${encodeURIComponent(trimmed)}`
    );
    cb(res.data.map((name) => ({ value: name })));
  } catch {
    cb([]);
  }
};

async function onProductSelect(item) {
  form.value.productName = item.value;
  try {
    const res = await api.post(
      `/products/paginated?pageNumber=1&pageSize=1&search=${encodeURIComponent(
        item.value
      )}`,
      {}
    );
    const first = res.data.items?.[0];
    if (first) {
      form.value.productId = first.id;
    }
  } catch {}
}

function openAdd() {
  editingId.value = null;
  form.value = {
    productId: null,
    productName: "",
    stockQuantity: 0,
    price: null,
    isAvailable: true,
  };
  dialogVisible.value = true;
}

function editProduct(p) {
  editingId.value = p.productId;
  form.value = { ...p };
  dialogVisible.value = true;
}

async function saveProduct() {
  const valid = await formRef.value.validate().catch(() => false);
  if (!valid) return;
  const payload = {
    productId: form.value.productId,
    stockQuantity: form.value.stockQuantity,
    price: form.value.price,
    isAvailable: form.value.isAvailable,
  };
  try {
    if (editingId.value) {
      await updatePharmacyProduct(pharmacyId, editingId.value, payload);
    } else {
      await addPharmacyProduct(pharmacyId, payload);
    }
    dialogVisible.value = false;
    products.value = await getPharmacyProducts(pharmacyId);
    ElMessage.success("Сохранено");
  } catch {}
}

function goProduct(id) {
  router.push({
    name: "AdminProductDetails",
    params: { id },
    query: route.query,
  });
}

async function removeProduct(id) {
  try {
    await ElMessageBox.confirm("Удалить товар?", "Подтверждение", {
      confirmButtonText: "Удалить",
      cancelButtonText: "Отмена",
      type: "warning",
    });
  } catch {
    return;
  }
  try {
    await deletePharmacyProduct(pharmacyId, id);
    products.value = await getPharmacyProducts(pharmacyId);
    ElMessage.success("Удалено");
  } catch {}
}
</script>
