<template>
  <div class="flex items-center w-full border border-gray-300 bg-white rounded-md relative">
<el-popover
  placement="bottom-start"
  width="auto"
  v-model:visible="categoryDropdownVisible"
  trigger="click"
  popper-class="category-dropdown"
>
  <!-- dropdown content -->
  <template #default>
    <div
      class="flex z-50 bg-white rounded-md overflow-hidden"
      @mouseenter="categoryDropdownVisible = true"
      @mouseleave="categoryDropdownVisible = false"
    >
      <ul class="w-60 divide-y">
        <li
          @click="resetCategory"
          class="px-4 py-3 text-sm font-medium text-gray-800 hover:bg-primary-50 cursor-pointer"
        >
          Все категории
        </li>
        <li
          v-for="cat in categories"
          :key="cat.id"
          @mouseenter="activeCategory = cat"
          @click="selectCategory(cat)"
          class="px-4 py-3 text-sm font-medium text-gray-800 hover:bg-primary-50 cursor-pointer"
        >
          {{ cat.name }}
        </li>
      </ul>

      <ul
        v-if="activeCategory?.subcategories?.length"
        class="w-64 border-l divide-y"
      >
        <li
          v-for="sub in activeCategory.subcategories"
          :key="sub.id"
          @click="selectCategory(sub)"
          class="px-4 py-3 text-sm text-gray-800 hover:bg-primary-50 cursor-pointer"
        >
          {{ sub.name }}
        </li>
      </ul>
    </div>
  </template>

  <!-- trigger button -->
  <template #reference>
    <div
      class="px-4 h-10 flex items-center gap-2 text-sm text-gray-700 font-medium cursor-pointer"
    >
      {{ selectedCategoryName }} <i class="fas fa-chevron-down text-xs"></i>
    </div>
  </template>
</el-popover>


    <div class="w-px h-6 bg-gray-300 mx-2"></div>

    <!-- Поиск -->
    <div class="relative flex-1">
      <el-input
        v-model="searchQuery"
        placeholder="Поиск товаров..."
        clearable
        @input="onSearchInput"
        @keyup.enter="() => goToSearch()"
        @focus="onFocus"
        @blur="onBlur"
        class="!h-10 !text-sm !border-none !rounded-none !shadow-none w-full"
      >
        <template #prefix>
          <i class="fas fa-search text-gray-400 ml-2"></i>
        </template>
      </el-input>

      <!-- Результаты поиска -->
      <ul
        v-if="showDropdown && searchResults.length"
        class="absolute z-50 left-0 right-0 mt-1 bg-white border border-gray-200 rounded-md shadow max-h-60 overflow-auto"
        @mouseenter="dropdownHovered = true"
        @mouseleave="dropdownHovered = false"
      >
        <li
          v-for="(result, index) in searchResults"
          :key="index"
          @click="goToSearch(result)"
          class="px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 cursor-pointer"
        >
          {{ result }}
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useCategoryStore } from '../stores/CategoryStore'
import api from '../utils/axios'
import { toSlug } from '../utils/slugify'

const router = useRouter()
const categoryStore = useCategoryStore()
const { categories, selectedCategoryName } = storeToRefs(categoryStore)

const searchQuery = ref('')
const searchResults = ref([])
const showDropdown = ref(false)
const dropdownHovered = ref(false)
const debounceTimer = ref(null)

const categoryDropdownVisible = ref(false)
const activeCategory = ref(null)

function toggleCategoryDropdown() {
  categoryDropdownVisible.value = !categoryDropdownVisible.value
}

function closeCategoryDropdown() {
  categoryDropdownVisible.value = false
  activeCategory.value = null
}

function selectCategory(category) {
  categoryStore.selectCategory(category.id, category.name)
  categoryStore.fetchPropertyFilters(category.id)
  categoryDropdownVisible.value = false
  activeCategory.value = null
  router.push({ path: `/products/catalog/${toSlug(category.name)}` })
}

function resetCategory() {
  categoryStore.resetCategory()
  router.push({ path: '/products/catalog' })
}

const fetchSearchSuggestions = async (query) => {
  const trimmed = query.trim()
  if (!trimmed || trimmed.length < 2) {
    searchResults.value = []
    showDropdown.value = false
    return
  }
  try {
    const response = await api.get(`/products/search-suggestions?query=${encodeURIComponent(query)}`)
    searchResults.value = response.data
    showDropdown.value = true
  } catch (err) {
    console.error('Ошибка при получении подсказок:', err)
  }
}

const onSearchInput = () => {
  clearTimeout(debounceTimer.value)
  debounceTimer.value = setTimeout(() => {
    fetchSearchSuggestions(searchQuery.value)
  }, 500)
}

const onFocus = () => {
  if (searchResults.value.length > 0) showDropdown.value = true
}

const onBlur = () => {
  requestAnimationFrame(() => {
    if (!dropdownHovered.value) showDropdown.value = false
  })
}

function goToSearch(queryFromList = null) {
  const query = typeof queryFromList === 'string'
    ? queryFromList
    : searchQuery.value.trim()

  if (!query) return

  router.push({ path: '/products/catalog', query: { search: query } })
  showDropdown.value = false
}
</script>

<style scoped>
ul::-webkit-scrollbar {
  width: 4px;
}

ul::-webkit-scrollbar-thumb {
  background-color: #ccc;
  border-radius: 2px;
}

:deep(.el-input__wrapper) {
  border: none !important;
  box-shadow: none !important;
  border-radius: 0 !important;
  background-color: transparent !important;
}

:deep(.el-input__wrapper.is-focus) {
  box-shadow: none !important;
  border-color: transparent !important;
}

:deep(.el-input__inner) {
  border: none !important;
  outline: none !important;
  box-shadow: none !important;
  background-color: transparent !important;
}

:deep(.el-input) {
  height: 100% !important;
}

</style>
