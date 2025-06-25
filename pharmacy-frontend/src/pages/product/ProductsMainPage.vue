<template>
  <div class="space-y-6">
    <!-- Сортировка + количество -->
    <div class="flex justify-between items-center">
      <div class="flex items-center gap-4 text-sm text-gray-700">
        <button
          class="hover:underline"
          :class="
            sort === 'datetime_desc' ? 'font-semibold text-primary-600' : ''
          "
          @click="changeSort('datetime_desc')"
        >
          Сначала новые
        </button>
        <button
          class="hover:underline"
          :class="
            sort === 'datetime_asc' ? 'font-semibold text-primary-600' : ''
          "
          @click="changeSort('datetime_asc')"
        >
          Сначала старые
        </button>
        <button
          class="hover:underline"
          :class="sort === 'price_asc' ? 'font-semibold text-primary-600' : ''"
          @click="changeSort('price_asc')"
        >
          Цена ↑
        </button>
        <button
          class="hover:underline"
          :class="sort === 'price_desc' ? 'font-semibold text-primary-600' : ''"
          @click="changeSort('price_desc')"
        >
          Цена ↓
        </button>
      </div>
      <div class="text-sm text-gray-600">Найдено товаров: {{ totalCount }}</div>
    </div>

    <!-- Товары -->
    <div v-if="loading" class="text-center py-10">Загрузка...</div>

    <div
      v-else-if="products.length === 0"
      class="text-center py-10 text-gray-500"
    >
      <i class="fas fa-box-open text-4xl mb-4"></i>
      <p class="text-lg font-medium">Товары не найдены</p>
      <p class="text-sm mt-1">Попробуйте изменить фильтры или сбросить поиск</p>
    </div>

    <div v-else class="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
      <ProductCard v-for="p in products" :key="p.id" :product="p" />
    </div>

    <!-- Пагинация -->
    <div
      v-if="totalCount > pageSize"
      class="mt-8 text-center flex justify-center"
    >
      <el-pagination
        layout="prev, pager, next"
        :total="totalCount"
        :current-page="pageNumber"
        :page-size="pageSize"
        @current-change="onPageChange"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, watch, computed, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import ProductCard from "../../components/cards/ProductCard.vue";
import { useProducts } from "../../composables/useProducts";
import { useCategoryStore } from "../../stores/CategoryStore";

const props = defineProps({
  filters: Object,
});

const route = useRoute();
const router = useRouter();
const categoryStore = useCategoryStore();
const searchQuery = ref(route.query.search || "");
const sort = ref(route.query.sort || "datetime_desc");

const { products, totalCount, pageNumber, pageSize, loading, fetchProducts } =
  useProducts();

const fetch = () => {
  const [sortBy, sortOrder] = sort.value.split("_");
  fetchProducts({
    page: pageNumber.value,
    size: 9,
    sortBy: sortBy === "datetime" ? "datetime" : sortBy,
    sortOrder,
    search: searchQuery.value || null,
    filters: props.filters || {},
  });
};

const onPageChange = (page) => {
  pageNumber.value = page;
  router.replace({ query: { ...route.query, page } });
  fetch();
};

const changeSort = (val) => {
  sort.value = val;
  pageNumber.value = 1;
  router.replace({ query: { ...route.query, page: 1, sort: val } });
  fetch();
};

onMounted(async () => {
  if (route.params.slug) {
    await categoryStore.fetchCategoryBySlug(route.params.slug);
  } else {
    categoryStore.resetCategory();
  }

  pageNumber.value = Number(route.query.page) || 1;
  sort.value = route.query.sort || "datetime_desc";

  const newCatIds = categoryStore.selectedCategoryId
    ? [categoryStore.selectedCategoryId]
    : [];
  if (JSON.stringify(props.filters.categoryIds) !== JSON.stringify(newCatIds)) {
    props.filters.categoryIds = newCatIds;
  }

  fetch();
});

watch(
  () => route.params.slug,
  async (slug) => {
    if (slug) {
      await categoryStore.fetchCategoryBySlug(slug);
    } else {
      categoryStore.resetCategory();
    }

    const newCatIds = categoryStore.selectedCategoryId
      ? [categoryStore.selectedCategoryId]
      : [];
    if (
      JSON.stringify(props.filters.categoryIds) !== JSON.stringify(newCatIds)
    ) {
      props.filters.categoryIds = newCatIds;
    }

    pageNumber.value = 1;
    router.replace({ query: { ...route.query, page: 1 } });
    fetch();
  }
);

watch(
  () => route.query.search,
  (newSearch) => {
    searchQuery.value = newSearch || "";
    pageNumber.value = 1;
    router.replace({ query: { ...route.query, page: 1 } });
    fetch();
  }
);

watch(
  () => route.query.sort,
  (newSort) => {
    const val = newSort || "datetime_desc";
    if (val !== sort.value) {
      sort.value = val;
      fetch();
    }
  }
);

watch(
  () => route.query.page,
  (val) => {
    const newPage = Number(val) || 1;
    if (newPage !== pageNumber.value) {
      pageNumber.value = newPage;
      fetch();
    }
  }
);

watch(
  () => props.filters,
  () => {
    pageNumber.value = 1;
    router.replace({ query: { ...route.query, page: 1 } });
    fetch();
  },
  { deep: true }
);
</script>
