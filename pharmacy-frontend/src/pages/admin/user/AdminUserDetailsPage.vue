<template>
  <div>
    <el-button type="text" @click="router.back()"
      ><i class="fas fa-arrow-left mr-1"></i> Назад</el-button
    >
    <h1 class="text-2xl font-semibold mb-6">Пользователь #{{ user?.id }}</h1>

    <div v-if="loading" class="text-center py-10">Загрузка...</div>
    <div v-else-if="!user" class="text-center py-10 text-gray-500">
      Пользователь не найден
    </div>
    <div v-else class="bg-white rounded-lg shadow p-6 space-y-4">
      <p><b>Email:</b> {{ user.email }}</p>
      <p><b>Почта подтверждена:</b> {{ user.emailVerified ? "Да" : "Нет" }}</p>
      <p><b>Роль:</b> {{ user.role }}</p>
      <p>
        <b>Имя:</b> {{ user.lastName }} {{ user.firstName }}
        {{ user.patronymic }}
      </p>
      <p><b>Телефон:</b> {{ user.phone || "—" }}</p>
      <p v-if="user.pharmacy"><b>Аптека:</b> {{ user.pharmacy.name }}</p>
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
