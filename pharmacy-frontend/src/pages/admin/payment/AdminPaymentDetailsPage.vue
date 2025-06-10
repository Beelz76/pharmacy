<template>
  <div class="max-w-5xl mx-auto">
    <div class="flex items-center gap-4 mb-8">
      <button
        @click="router.back()"
        class="flex items-center text-primary-600 hover:text-primary-700 text-base group transition"
      >
        <i
          class="text-xl fas fa-arrow-left mr-2 group-hover:-translate-x-1 duration-150"
        ></i>
      </button>
      <h2 class="text-2xl font-bold tracking-tight">
        Платеж №{{ payment?.id }}
      </h2>
    </div>

    <div v-if="loading" class="text-center py-10">Загрузка...</div>
    <div v-else-if="!payment" class="text-center py-10 text-gray-500">
      Платеж не найден
    </div>
    <div v-else class="space-y-6">
      <div class="bg-white border rounded-xl p-6 shadow-sm space-y-3">
        <p class="text-base text-gray-700">
          <span class="font-medium">ID:</span> {{ payment.id }}
        </p>
        <p class="text-base text-gray-700">
          <span class="font-medium">Номер заказа:</span>
          {{ payment.orderNumber }}
        </p>
        <p class="text-base text-gray-700">
          <span class="font-medium">Аптека:</span> {{ payment.pharmacyName }}
        </p>
        <p class="text-base text-gray-700">
          <span class="font-medium">Сумма:</span>
          {{ payment.amount.toFixed(2) }} ₽
        </p>
        <p class="text-base text-gray-700">
          <span class="font-medium">Метод:</span> {{ payment.method }}
        </p>
        <p v-if="payment.externalPaymentId" class="text-base text-gray-700">
          <span class="font-medium">Внешний GUID:</span>
          {{ payment.externalPaymentId }}
        </p>
        <p class="text-base text-gray-700">
          <span class="font-medium">Статус:</span> {{ payment.status }}
        </p>
        <p v-if="payment.transactionDate" class="text-base text-gray-700">
          <span class="font-medium">Дата транзакции:</span>
          {{ formatDate(payment.transactionDate) }}
        </p>
        <button
          class="text-primary-600 hover:text-primary-700 text-base"
          @click="goOrder(payment.orderId)"
        >
          Перейти к заказу
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { getPaymentById } from "/src/services/PaymentService";

const router = useRouter();
const route = useRoute();
const payment = ref(null);
const loading = ref(false);

const formatDate = (iso) => (iso ? new Date(iso).toLocaleString("ru-RU") : "");

const goOrder = (id) => {
  router.push({ name: "AdminOrderDetails", params: { id } });
};

onMounted(async () => {
  loading.value = true;
  try {
    payment.value = await getPaymentById(route.params.id);
  } finally {
    loading.value = false;
  }
});
</script>
