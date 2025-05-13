<template>
  <header class="sticky top-0 z-50 bg-white/80 backdrop-blur-md border-b border-gray-200">
    <div class="max-w-7xl mx-auto px-2">
      <div class="flex items-center h-16 gap-6">
        <!-- Логотип -->
        <div class="flex items-center flex-shrink-0 space-x-6">
          <router-link to="/" class="flex items-center">
            <i class="fas fa-pills text-primary-600 text-2xl mr-2"></i>
            <span class="text-2xl font-bold text-gray-900">MediCare</span>
          </router-link>

          <!-- Навигация -->
          <nav class="hidden md:flex space-x-8 relative">
            <router-link
              to="/"
              class="text-base font-medium"
              :class="$route.path === '/' ? 'text-primary-600 border-b-2 border-primary-500' : 'text-gray-500 hover:text-gray-700'"
            >
              Главная
            </router-link>

            <router-link
              to="/about"
              class="text-base font-medium"
              :class="$route.path === '/about' ? 'text-primary-600 border-b-2 border-primary-500' : 'text-gray-500 hover:text-gray-700'"
            >
              О нас
            </router-link>
          </nav>
        </div>

<!-- Категория + Поиск -->
<div class="relative flex-grow hidden md:flex items-center rounded-md overflow-visible border border-gray-300 bg-white">
  <!-- Выпадающий список категорий -->
  <el-dropdown
    
    v-model:visible="categoryDropdownVisible"
  >
    <template #default>
      <div
        class="px-4 h-10 text-sm font-medium text-gray-700 flex items-center gap-2 cursor-pointer select-none focus:outline-none"
        @click="toggleCategoryDropdown"
      >
        {{ selectedCategoryName }} <i class="fas fa-chevron-down text-xs"></i>
      </div>
    </template>

    <template #dropdown>
      <div
        class="inline-flex rounded-md shadow-lg z-50 overflow-hidden"
        @mouseenter="categoryDropdownVisible = true"
        @mouseleave="closeCategoryDropdown"
      >
        <ul class="w-60 border-r divide-y bg-white max-h-72 overflow-auto">
          <li
            @click="resetCategory"
            class="px-4 py-3 text-sm hover:bg-primary-50 cursor-pointer font-medium text-gray-800 whitespace-nowrap "
          >
            Все категории
          </li>
          <li
            v-for="cat in categories"
            :key="cat.id"
            @mouseenter="activeCategory = cat"
            @click="selectCategory(cat)"
            class="px-4 py-3 text-sm hover:bg-primary-50 cursor-pointer font-medium text-gray-800 whitespace-nowrap"
          >
            {{ cat.name }}
          </li>
        </ul>

        <ul
          v-if="activeCategory?.subcategories?.length"
          class="divide-y bg-white max-h-72 overflow-auto"
          :style="{ minWidth: '16rem' }"
        >
          <li
            v-for="sub in activeCategory.subcategories"
            :key="sub.id"
            @click="selectCategory(sub)"
            class="px-4 py-3 text-sm hover:bg-primary-50 cursor-pointer text-gray-800 whitespace-nowrap"
          >
            {{ sub.name }}
          </li>
        </ul>
      </div>
    </template>
  </el-dropdown>

  <!-- Разделительная линия -->
  <div class="w-px h-6 bg-gray-300 mx-2"></div>

  <!-- Поле поиска -->
  <div class="relative flex-1">
    <el-input
      v-model="searchQuery"
      placeholder="Поиск товаров..."
      clearable
      @input="onSearchInput"
      @keyup.enter="goToSearch"
      @focus.native="onFocus"
      @blur.native="onBlur"
      class="!h-10 !text-sm !border-none !rounded-none !shadow-none w-full"
    >
      <template #prefix>
        <i class="fas fa-search text-gray-400 ml-2"></i>
      </template>
    </el-input>

    <!-- Выпадающие подсказки -->
    <ul
      v-if="showDropdown && searchResults.length > 0"
      class="absolute z-50 left-0 right-0 mt-1 bg-white border border-gray-200 rounded-md shadow-md max-h-60 overflow-auto"
      @mouseenter="dropdownHovered = true"
      @mouseleave="dropdownHovered = false"
    >
      <li
        v-for="(result, index) in searchResults"
        :key="index"
        @click="goToSearch(result)"
        class="px-4 py-2 hover:bg-gray-100 cursor-pointer text-sm text-gray-700"
      >
        {{ result }}
      </li>
    </ul>
  </div>
</div>

<!-- Иконки и профиль -->
<div class="flex items-center flex-shrink-0 space-x-6">
  <template v-if="isAuthenticated">
    <!-- Избранное -->
    <div class="relative flex flex-col items-center">
      <router-link to="/account/favorites" class="text-xl text-gray-600 hover:text-primary-600">
        <i class="fas fa-heart"></i>
      </router-link>
      <span
        v-if="favoritesCount > 0"
        class="absolute top-5 text-[10px] text-gray-700 font-semibold"
      >
        {{ favoritesCount }}
      </span>
    </div>

    <!-- Корзина -->
    <div class="relative flex flex-col items-center">
      <router-link
        to="/cart"
        class="text-xl"
        :class="$route.path === '/cart' ? 'text-primary-600' : 'text-gray-600 hover:text-primary-600'"
      >
        <i class="fas fa-shopping-cart"></i>
      </router-link>
      <span
        v-if="cartCount > 0"
        class="absolute top-5 text-[10px] text-gray-700 font-semibold"
      >
        {{ cartCount }}
      </span>
    </div>

    <!-- Профиль -->
    <el-dropdown>
      <template #default>
        <button class="bg-primary-600 hover:bg-primary-700 text-white text-sm font-medium px-4 py-2 rounded flex items-center gap-2 transition focus:outline-none">
          <i class="fas fa-user"></i>
          Профиль
          <i class="fas fa-chevron-down text-xs"></i>
        </button>
      </template>
      <template #dropdown>
        <el-dropdown-menu>
          <el-dropdown-item>
            <router-link to="/account">Личный кабинет</router-link>
          </el-dropdown-item>
          <el-dropdown-item>
            <router-link to="/account/orders">История заказов</router-link>
          </el-dropdown-item>
          <el-dropdown-item @click="handleLogout">Выйти</el-dropdown-item>
        </el-dropdown-menu>
      </template>
    </el-dropdown>
  </template>

  <template v-else>
    <el-button
      type="primary"
      class="!bg-primary-600 !h-10 hover:!bg-primary-700"
      @click="openAuthModal"
    >
      <i class="fas fa-user mr-2"></i> Войти
    </el-button>
  </template>
</div>

      </div>
    </div>
  </header>
</template>


<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { storeToRefs } from 'pinia'
import { useAuthStore } from '../stores/AuthStore'
import { useFavoritesStore } from '../stores/FavoritesStore'
import { useCartStore } from '../stores/CartStore'
import { useCategoryStore } from '../stores/CategoryStore'
import api from '../utils/axios'

const auth = useAuthStore()
const favorites = useFavoritesStore()
const cart = useCartStore()
const categoryStore = useCategoryStore()

const { isAuthenticated } = storeToRefs(auth)
const { favoritesCount } = storeToRefs(favorites)
const { cartCount } = storeToRefs(cart)
const { categories } = storeToRefs(categoryStore)
const { selectedCategoryName } = storeToRefs(categoryStore)

const activeCategory = ref(null)
const categoryDropdownVisible = ref(false)

const $route = useRoute()
const router = useRouter()
const searchQuery = ref('')
const searchResults = ref([])
const showDropdown = ref(false)
const dropdownHovered = ref(false)
const debounceTimer = ref(null)

watch(
  () => $route.path,
  (path) => {
    if (!path.includes('/products')) {
      categoryStore.resetCategory()
    }
  }
)

watch(
  () => auth.role,
  (newRole) => {
    if (isAuthenticated.value && newRole === 'User') {
      favorites.fetchCount()
      cart.fetchCartCount()
    }
  },
  { immediate: true }
)

onMounted(() => {
  categoryStore.fetchCategories()
})

function selectCategory(category) {
  categoryStore.selectCategory(category.id, category.name)
  router.push({ path: '/products', query: { categoryId: category.id } })
}

function resetCategory() {
  categoryStore.resetCategory()
  router.push({ path: '/products' })
}

function toggleCategoryDropdown() {
  categoryDropdownVisible.value = !categoryDropdownVisible.value
}

function closeCategoryDropdown() {
  categoryDropdownVisible.value = false
  activeCategory.value = null
}

function openAuthModal() {
  auth.setReturnUrl(router.currentRoute.value.fullPath)
  window.dispatchEvent(new Event('unauthorized'))
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
  if (searchResults.value.length > 0) {
    showDropdown.value = true
  }
}

const onBlur = () => {
  requestAnimationFrame(() => {
    if (!dropdownHovered.value) {
      showDropdown.value = false
    }
  })
}

function goToSearch(value) {
  const query = typeof value === 'string' ? value : searchQuery.value
  if (query.trim()) {
    showDropdown.value = false
    router.push({ path: '/products', query: { search: query.trim() } })
  }
}

function handleLogout() {
  auth.logout()
  router.push('/')
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

/* Убираем внутренние обводки у поля поиска */
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

/* Дополнительно: выравнивание высоты и убираем разделение */
:deep(.el-input) {
  height: 100% !important;
}
</style>
