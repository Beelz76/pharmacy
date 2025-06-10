<template>
  <div>
    <h1 class="text-2xl font-semibold mb-2">Товары</h1>
    <div class="mb-4 text-gray-600">Всего товаров: {{ totalCount }}</div>

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
        <el-form-item label="Категория">
          <el-select
            v-model="filters.categoryIds"
            multiple
            clearable
            filterable
            class="!w-60"
          >
            <el-option
              v-for="c in flatCategories"
              :key="c.id"
              :value="c.id"
              :label="c.name"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Производитель">
          <el-select
            v-model="filters.manufacturerIds"
            multiple
            clearable
            filterable
            class="!w-60"
          >
            <el-option
              v-for="m in manufacturers"
              :key="m.id"
              :value="m.id"
              :label="m.name"
            />
          </el-select>
        </el-form-item>
        <el-form-item
          v-for="filter in propertyFilterOptions"
          :key="filter.key"
          :label="filter.label"
        >
          <el-select
            v-model="filters.propertyFilters[filter.key]"
            multiple
            clearable
            class="!w-60"
          >
            <el-option
              v-for="val in filter.values"
              :key="val"
              :value="val"
              :label="val"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Доступен">
          <el-switch v-model="filters.isAvailable" />
        </el-form-item>
        <el-form-item label="Сортировка">
          <el-select v-model="sort" class="!w-40">
            <el-option label="Сначала новые" value="datetime_desc" />
            <el-option label="Сначала старые" value="datetime_asc" />
            <el-option label="Цена ↑" value="price_asc" />
            <el-option label="Цена ↓" value="price_desc" />
          </el-select>
        </el-form-item>
        <el-form-item>
          <el-button type="primary" plain @click="fetch">Поиск</el-button>
          <el-button @click="resetFilters">Сбросить</el-button>
        </el-form-item>
      </el-form>
    </div>

    <div class="flex justify-between mb-4">
      <el-button type="primary" @click="createProduct">
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
            <th class="px-6 py-5 font-semibold">Категория</th>
            <th class="px-6 py-5 font-semibold">Цена</th>
            <th class="px-6 py-5 font-semibold">Доступен</th>
            <th class="px-6 py-5 font-semibold">Действия</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-for="p in products" :key="p.id">
            <td class="px-6 py-4">{{ p.id }}</td>
            <td class="px-6 py-4">{{ p.name }}</td>
            <td class="px-6 py-4">{{ p.categoryName }}</td>
            <td class="px-6 py-4">{{ p.price }}</td>
            <td class="px-6 py-4">{{ p.isAvailable ? "Да" : "Нет" }}</td>
            <td class="px-6 py-4">
              <div class="flex gap-2">
                <el-button size="small" @click="editProduct(p.id)">
                  <i class="fas fa-edit" />
                </el-button>
                <el-button
                  size="small"
                  type="danger"
                  @click="removeProduct(p.id)"
                >
                  <i class="fas fa-trash" />
                </el-button>
              </div>
            </td>
          </tr>
          <tr v-if="!loading && products.length === 0">
            <td colspan="6" class="text-center py-6 text-gray-500">
              Товары не найдены
            </td>
          </tr>
          <tr v-if="loading">
            <td colspan="6" class="text-center py-6 text-gray-500">
              Загрузка...
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import { useRouter, useRoute } from "vue-router";
import { useProducts } from "/src/composables/useProducts";
import { getAllCategories } from "/src/services/CategoryService";
import { getManufacturers } from "/src/services/ManufacturerService";
import ProductService from "/src/services/ProductService";
import { ElMessageBox, ElMessage } from "element-plus";

const router = useRouter();
const route = useRoute();

const { products, totalCount, pageNumber, pageSize, loading, fetchProducts } =
  useProducts();

const sort = ref("datetime_desc");

const search = ref(route.query.search || "");
const filters = ref({
  categoryIds: [],
  manufacturerIds: [],
  isAvailable: true,
  propertyFilters: {},
});

const categories = ref([]);
const manufacturers = ref([]);
const flatCategories = ref([]);
const propertyFilterOptions = ref([]);

function flatten(list, prefix = "", arr = []) {
  for (const c of list) {
    arr.push({ id: c.id, name: prefix + c.name });
    if (c.subcategories?.length) {
      flatten(c.subcategories, prefix + c.name + " / ", arr);
    }
  }
  return arr;
}

onMounted(async () => {
  categories.value = await getAllCategories();
  flatCategories.value = flatten(categories.value);
  manufacturers.value = await getManufacturers();
  pageNumber.value = Number(route.query.page) || 1;
  pageSize.value = Number(route.query.size) || pageSize.value;
  fetch();
});

watch(
  () => filters.value.categoryIds,
  async (val) => {
    if (val.length === 1) {
      propertyFilterOptions.value = await ProductService.getFilterValues(
        val[0]
      );
    } else {
      propertyFilterOptions.value = [];
      filters.value.propertyFilters = {};
    }
  },
  { deep: true }
);

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

function fetch() {
  const filterPayload = {
    categoryIds: filters.value.categoryIds,
    manufacturerIds: filters.value.manufacturerIds,
    isAvailable: filters.value.isAvailable,
    propertyFilters: filters.value.propertyFilters,
  };
  const [sortBy, sortOrder] = sort.value.split("_");
  fetchProducts({
    page: pageNumber.value,
    size: pageSize.value,
    sortBy: sortBy === "datetime" ? "datetime" : sortBy,
    sortOrder,
    search: search.value || null,
    filters: filterPayload,
  });
}

function resetFilters() {
  search.value = "";
  filters.value = {
    categoryIds: [],
    manufacturerIds: [],
    propertyFilters: {},
    isAvailable: true,
  };
  sort.value = "datetime_desc";
  pageNumber.value = 1;
  fetch();
}

function createProduct() {
  router.push({ name: "AdminProductCreate" });
}

function editProduct(id) {
  router.push({ name: "AdminProductDetails", params: { id } });
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
    await ProductService.delete(id);
    ElMessage.success("Удалено");
    fetch();
  } catch (e) {
    ElMessage.error(
      `${e.response?.status} ${e.response?.data?.message || e.message}`
    );
  }
}
</script>
