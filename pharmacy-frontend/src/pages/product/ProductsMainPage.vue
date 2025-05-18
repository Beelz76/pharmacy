<template>
  <div class="space-y-6">

    <!-- Сортировка + количество -->
    <div class="flex justify-between items-center">
      <div class="flex items-center gap-4 text-sm text-gray-700">
        <button
          class="hover:underline"
          :class="sort === 'datetime_desc' ? 'font-semibold text-primary-600' : ''"
          @click="changeSort('datetime_desc')"
        >Сначала новые</button>
        <button
          class="hover:underline"
          :class="sort === 'datetime_asc' ? 'font-semibold text-primary-600' : ''"
          @click="changeSort('datetime_asc')"
        >Сначала старые</button>
        <button
          class="hover:underline"
          :class="sort === 'price_asc' ? 'font-semibold text-primary-600' : ''"
          @click="changeSort('price_asc')"
        >Цена ↑</button>
        <button
          class="hover:underline"
          :class="sort === 'price_desc' ? 'font-semibold text-primary-600' : ''"
          @click="changeSort('price_desc')"
        >Цена ↓</button>
      </div>
      <div class="text-sm text-gray-600">
        Найдено товаров: {{ totalCount }}
      </div>
    </div>

<!-- Товары -->
<div v-if="loading" class="text-center py-10">Загрузка...</div>

<div v-else-if="products.length === 0" class="text-center py-10 text-gray-500">
  <i class="fas fa-box-open text-4xl mb-4"></i>
  <p class="text-lg font-medium">Товары не найдены</p>
  <p class="text-sm mt-1">Попробуйте изменить фильтры или сбросить поиск</p>
</div>

<div v-else class="grid sm:grid-cols-2 lg:grid-cols-3 gap-6">
  <ProductCard v-for="p in products" :key="p.id" :product="p" />
</div>

<!-- Пагинация -->
<div v-if="totalCount > pageSize" class="mt-8 text-center flex justify-center">
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
import { ref, watch, computed } from 'vue'
import { useRoute } from 'vue-router'
import ProductCard from '../../components/cards/ProductCard.vue'
import { useProducts } from '../../composables/useProducts'
import { useCategoryStore } from '../../stores/CategoryStore'

const props = defineProps({
  filters: Object
})

const route = useRoute()
const categoryStore = useCategoryStore()
const searchQuery = ref(route.query.search || '')
const sort = ref('datetime_desc')

const {
  products,
  totalCount,
  pageNumber,
  pageSize,
  loading,
  fetchProducts
} = useProducts()

const fetch = () => {
  const [sortBy, sortOrder] = sort.value.split('_')
  fetchProducts({
    page: pageNumber.value,
    size: 9,
    sortBy: sortBy === 'datetime' ? 'datetime' : sortBy,
    sortOrder,
    search: searchQuery.value || null,
    filters: props.filters || {}
  })
}

const onPageChange = (page) => {
  pageNumber.value = page
  fetch()
}

const changeSort = (val) => {
  sort.value = val
  pageNumber.value = 1
  fetch()
}

watch(
  () => route.params.slug,
  async (slug) => {
    if (slug) {
      await categoryStore.fetchCategoryBySlug(slug)
    } else {
      categoryStore.resetCategory()
    }

    props.filters.categoryIds = categoryStore.selectedCategoryId
      ? [categoryStore.selectedCategoryId]
      : []

    pageNumber.value = 1
    fetch()
  },
  { immediate: true }
)

watch(() => route.query.search, (newSearch) => {
  searchQuery.value = newSearch || ''
  pageNumber.value = 1
  fetch()
}, { immediate: true })

watch(
  () => props.filters,
  () => {
    pageNumber.value = 1
    fetch()
  },
  { immediate: false, deep: true }
)
</script>
