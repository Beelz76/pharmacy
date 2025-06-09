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
        Пользователь #{{ user?.id }}
      </h2>
    </div>

    <div v-if="loading" class="text-center py-10">Загрузка...</div>
    <div v-else-if="!user" class="text-center py-10 text-gray-500">
      Пользователь не найден
    </div>
    <div v-else class="bg-white border rounded-xl p-6 shadow-sm space-y-3">
      <p class="text-base text-gray-700">
        <span class="font-medium">Email:</span> {{ user.email }}
      </p>
      <p class="text-base text-gray-700">
        <span class="font-medium">Почта подтверждена:</span>
        {{ user.emailVerified ? "Да" : "Нет" }}
      </p>
      <p class="text-base text-gray-700">
        <span class="font-medium">Роль:</span> {{ user.role }}
      </p>
      <p class="text-base text-gray-700">
        <span class="font-medium">Имя:</span> {{ user.lastName }}
        {{ user.firstName }} {{ user.patronymic }}
      </p>
      <p class="text-base text-gray-700">
        <span class="font-medium">Телефон:</span> {{ user.phone || "—" }}
      </p>
      <p v-if="user.pharmacy" class="text-base text-gray-700">
        <span class="font-medium">Аптека:</span> {{ user.pharmacy.name }}
      </p>
      <button
        class="text-primary-600 hover:text-primary-700 text-base underline"
        @click="goToUserOrders(user.id)"
      >
        Заказы пользователя
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { getUserById } from "/src/services/UserService";

const route = useRoute();
const router = useRouter();
const user = ref(null);
const loading = ref(false);

onMounted(async () => {
  loading.value = true;
  try {
    user.value = await getUserById(route.params.id);
  } catch (e) {
    user.value = null;
  } finally {
    loading.value = false;
  }
});
</script>
