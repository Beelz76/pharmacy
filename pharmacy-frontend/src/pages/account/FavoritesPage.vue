<template>
  <div>
    <h2 class="text-2xl font-bold mb-6">Избранное</h2>

    <div v-if="favorites.length === 0" class="text-gray-500 text-center">
      У вас пока нет избранных товаров.
    </div>

    <div v-else>
      <div class="space-y-6">
        <FavoriteCard
          v-for="product in paginatedFavorites"
          :key="product.productId"
          :product="product"
        />
      </div>

      <div v-if="totalPages > 1" class="flex justify-center mt-10">
        <el-pagination
          layout="prev, pager, next"
          :total="favorites.length"
          :page-size="pageSize"
          v-model:current-page="currentPage"
          class="!text-xl scale-110"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, computed, ref } from 'vue'
import { useFavoritesStore } from '/src/store/FavoritesStore'
import FavoriteCard from '/src/components/cards/FavoritesCard.vue'

const store = useFavoritesStore()

const currentPage = ref(1)
const pageSize = 4

onMounted(() => {
  store.fetchFavorites()
})

const favorites = computed(() => store.favorites || [])

const totalPages = computed(() => Math.ceil(favorites.value.length / pageSize))

const paginatedFavorites = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return favorites.value.slice(start, start + pageSize)
})
</script>

<style scoped>
</style>
