<template>
  <div>
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-xl font-semibold">Статусы заказа</h2>
    </div>
    <el-table :data="list" style="width: 100%">
      <el-table-column prop="id" label="ID" width="80" />
      <el-table-column prop="name" label="Название" />
      <el-table-column prop="description" label="Описание" />
    </el-table>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { getOrderStatuses } from "/src/services/OrderService";

const list = ref([]);

async function load() {
  list.value = await getOrderStatuses();
}

onMounted(load);
</script>
