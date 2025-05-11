<template>
  <section class="py-12 bg-white">
    <div class="max-w-7xl mx-auto px-4 text-center">
      <p class="text-primary-600 font-semibold uppercase">Категории</p>
      <h2 class="text-3xl font-extrabold text-gray-900 mt-2">Выберите по назначению</h2>
    </div>

    <div v-if="topLevelCategories.length" class="mt-10 max-w-7xl mx-auto">
      <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
        <div
          v-for="category in topLevelCategories"
          :key="category.id"
          @click="onCategoryClick(category)"
          class="bg-gray-50 rounded-pg p-6 text-center shadow hover:bg-primary-50 transition cursor-pointer"
        >
          <div class="flex justify-center mb-4">
            <div class="bg-primary-600 text-white p-3 rounded-full inline-flex items-center justify-center">
              <i class="fas fa-capsules text-xl"></i>
            </div>
          </div>
          <h3 class="text-lg font-medium text-gray-900">{{ category.name }}</h3>
          <p class="mt-1 text-sm text-gray-500">{{ category.description }}</p>
        </div>
      </div>
    </div>

    <div v-else class="flex justify-center mt-10">
      <LoadingSpinner size="lg" color="primary" class="mx-auto mt-6" />
    </div>
  </section>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useCategoryStore } from '../store/CategoryStore'
import LoadingSpinner from '../components/LoadingSpinner.vue'

const categoryStore = useCategoryStore()
const router = useRouter()

const topLevelCategories = computed(() =>
  categoryStore.categories
)

function onCategoryClick(category) {
  categoryStore.selectCategory(category.id, category.name)
  router.push({ path: '/products', query: { categoryId: category.id } })
}
</script>
