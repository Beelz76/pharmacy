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
        Склад аптеки #{{ pharmacy?.id }}
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
              <td class="px-6 py-4">{{ p.productName }}</td>
              <td class="px-6 py-4">{{ p.stockQuantity }}</td>
              <td class="px-6 py-4">{{ p.price }}</td>
              <td class="px-6 py-4">{{ p.isAvailable ? "Да" : "Нет" }}</td>
              <td class="px-6 py-4">
                <div class="flex justify-start gap-2">
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
      <el-form label-width="120px">
        <el-form-item label="ID товара" v-if="!editingId">
          <el-input v-model.number="form.productId" type="number" />
        </el-form-item>
        <el-form-item label="Количество">
          <el-input v-model.number="form.stockQuantity" type="number" />
        </el-form-item>
        <el-form-item label="Цена">
          <el-input v-model.number="form.price" type="number" />
        </el-form-item>
        <el-form-item label="Доступен">
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
import { ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  getPharmacyById,
  getPharmacyProducts,
  addPharmacyProduct,
  updatePharmacyProduct,
  deletePharmacyProduct,
} from "/src/services/PharmacyService";
import { ElMessageBox, ElMessage } from "element-plus";

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
  stockQuantity: 0,
  price: 0,
  isAvailable: true,
});

onMounted(async () => {
  loading.value = true;
  try {
    pharmacy.value = await getPharmacyById(pharmacyId);
    products.value = await getPharmacyProducts(pharmacyId);
  } finally {
    loading.value = false;
  }
});

function openAdd() {
  editingId.value = null;
  form.value = {
    productId: null,
    stockQuantity: 0,
    price: 0,
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
