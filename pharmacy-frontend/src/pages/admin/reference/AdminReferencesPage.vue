<template>
  <div>
    <h1 class="text-2xl font-semibold mb-6">Справочники</h1>
    <div class="flex gap-8">
      <el-tree
        class="w-64"
        :data="treeData"
        node-key="id"
        :default-expand-all="true"
        @node-click="onNodeClick"
      />
      <router-view />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const treeData = ref([
  {
    id: 'categories',
    label: 'Категории товаров',
  },
  {
    id: 'manufacturers',
    label: 'Производители',
  },
])

function onNodeClick(data) {
  if (data.id === 'categories') {
    router.push({ name: 'AdminCategories' })
  } else if (data.id === 'manufacturers') {
    router.push({ name: 'AdminManufacturers' })
  }
}
</script>