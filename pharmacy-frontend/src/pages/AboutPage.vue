<template>
  <div class="max-w-5xl mx-auto py-16 px-4">
    <!-- Заголовок -->
    <h1 class="text-3xl font-bold text-center mb-6">О нас</h1>
    <p class="text-gray-700 text-center text-lg max-w-2xl mx-auto mb-12">
      Мы — онлайн-аптека нового поколения. Быстрый поиск, удобное оформление и забота о вашем здоровье — всё это в одном сервисе.
    </p>

    <!-- Преимущества -->
    <div class="grid md:grid-cols-3 gap-6 mb-16 text-sm text-gray-600">
      <div class="text-center text-lg">
        <i class="fas fa-capsules text-primary-600 text-3xl mb-2"></i>
        <p>Широкий ассортимент</p>
      </div>
      <div class="text-center text-lg">
        <i class="fas fa-check-circle text-primary-600 text-3xl mb-2"></i>
        <p>Оригинальные и сертифицированные препараты</p>
      </div>
      <div class="text-center text-lg">
        <i class="fas fa-truck text-primary-600 text-3xl mb-2"></i>
        <p>Доставка и самовывоз</p>
      </div>
    </div>

    <!-- Инструкция -->
    <div class="mb-20">
      <h2 class="text-2xl font-bold text-center mb-8">Как оформить заказ</h2>
      <div class="space-y-6">
        <div v-for="(step, index) in steps" :key="index" class="flex gap-4 items-start">
          <div class="w-8 h-8 rounded-full bg-primary-600 text-white flex items-center justify-center text-sm font-semibold">
            {{ index + 1 }}
          </div>
          <div class="text-gray-700 text-lg">
            <p class="font-medium">{{ step.title }}</p>
            <p>{{ step.description }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Контакты -->
    <div class="text-center text-lg text-gray-600 space-y-3">
      <p><i class="fas fa-envelope mr-1 text-primary-600"></i> support@medicare.ru</p>
      <p><i class="fas fa-phone mr-1 text-primary-600"></i> +7 (495) 123-45-67</p>
    </div>
  </div>
</template>

<script setup>
const steps = [
  {
    title: 'Выберите товары',
    description: 'Добавьте нужные препараты в корзину через поиск или категории.',
  },
  {
    title: 'Перейдите к оформлению',
    description: 'Выберите город и способ получения — доставка или самовывоз из аптеки.',
  },
  {
    title: 'Заполните адрес (при доставке)',
    description: 'Укажите адрес и при необходимости оставьте комментарий для курьера.',
  },
  {
    title: 'Выберите способ оплаты',
    description: 'Оплатите онлайн или выберите оплату при получении.',
  },
  {
    title: 'Подтвердите заказ',
    description: 'Проверьте все данные и подтвердите заказ.',
  },
  {
    title: 'Получите заказ',
    description: 'Курьер доставит ваш заказ или вы сможете забрать его в выбранной аптеке.',
  },
]
</script>
