<template>
  <div>
    <div class="flex items-center justify-between mb-4">
      <h2 class="text-2xl font-semibold">Статусы оплаты</h2>
    </div>
    <div class="overflow-x-auto rounded-lg shadow border bg-white">
      <table class="min-w-full table-fixed divide-y divide-gray-200 text-sm">
        <thead
          class="bg-secondary-50 text-left text-secondary-700 uppercase text-sm"
        >
          <tr>
            <th class="px-6 py-5 font-semibold">ID</th>
            <th class="px-6 py-5 font-semibold">Название</th>
            <th class="px-6 py-5 font-semibold">Описание</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr v-for="s in list" :key="s.id">
            <td class="px-6 py-4">{{ s.id }}</td>
            <td class="px-6 py-4">{{ s.name }}</td>
            <td class="px-6 py-4">{{ s.description }}</td>
          </tr>
          <tr v-if="list.length === 0">
            <td colspan="3" class="text-center py-6 text-gray-500">
              Нет данных
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { getPaymentStatuses } from "/src/services/PaymentReferenceService";

const list = ref([]);

async function load() {
  list.value = await getPaymentStatuses();
}

onMounted(load);
</script>
