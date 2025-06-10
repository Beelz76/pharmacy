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
        Платёж №{{ payment?.id }}
      </h2>
    </div>

    <div v-if="loading" class="text-center py-10 text-gray-500 text-lg">
      <i class="fas fa-spinner fa-spin mr-2"></i>Загрузка данных о платеже...
    </div>

    <div v-else-if="!payment" class="text-center py-10 text-gray-500 text-lg">
      <i class="fas fa-exclamation-circle mr-2"></i>Платёж не найден
    </div>

    <div v-else class="space-y-6">
      <div class="bg-white border rounded-xl p-6 shadow-sm">
        <div
          class="grid grid-cols-1 sm:grid-cols-2 gap-y-4 gap-x-8 text-gray-700"
        >
          <p><span class="font-medium">ID:</span> {{ payment.id }}</p>
          <p>
            <span class="font-medium">Номер заказа:</span>
            {{ payment.orderNumber }}
          </p>
          <p class="sm:col-span-2">
            <span class="font-medium">Аптека:</span> {{ payment.pharmacyName }}
          </p>
          <p>
            <span class="font-medium">Сумма:</span>
            {{ payment.amount.toFixed(2) }} ₽
          </p>
          <p><span class="font-medium">Метод:</span> {{ payment.method }}</p>
          <p v-if="payment.externalPaymentId" class="sm:col-span-2">
            <span class="font-medium">Внешний GUID:</span>
            {{ payment.externalPaymentId }}
          </p>
          <p><span class="font-medium">Статус:</span> {{ payment.status }}</p>
          <p v-if="payment.transactionDate">
            <span class="font-medium">Дата транзакции:</span>
            {{ formatDate(payment.transactionDate) }}
          </p>
        </div>

        <div class="mt-6 text-right">
          <el-button type="primary" @click="goOrder(payment.orderId)">
            <i class="fas fa-receipt mr-2"></i>Перейти к заказу
          </el-button>
        </div>
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
