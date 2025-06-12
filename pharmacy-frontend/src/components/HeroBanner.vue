<template>
  <section class="hero-gradient">
    <div
      class="max-w-7xl mx-auto py-20 px-4 sm:py-28 sm:px-6 lg:px-8 text-center"
    >
      <h1
        class="text-4xl sm:text-5xl md:text-6xl font-extrabold tracking-tight text-gray-900 leading-tight mb-6 animate-fade-in"
      >
        <span class="block">Ваше здоровье</span>
        <span class="block text-primary-600">наш главный приоритет</span>
      </h1>

      <p
        class="mt-4 max-w-2xl mx-auto text-lg sm:text-xl text-gray-600 animate-fade-in delay-150"
      >
        Заказывайте лекарства онлайн, выбирайте доставку или аптеку рядом и
        получайте без очередей
      </p>

      <div class="mt-8 animate-fade-in delay-300">
        <a
          href="/products/catalog"
          class="inline-flex items-center justify-center px-8 py-4 border border-transparent text-lg font-medium rounded-lg shadow-sm text-white bg-primary-600 hover:bg-primary-700 transition"
        >
          Перейти к товарам
        </a>
      </div>
    </div>
  </section>
</template>

<style scoped>
.hero-gradient {
  background: linear-gradient(
    135deg,
    rgba(14, 165, 233, 0.1) 0%,
    rgba(139, 92, 246, 0.1) 100%
  );
}

/* Плавные появления */
@keyframes fade-in {
  0% {
    opacity: 0;
    transform: translateY(10px);
  }
  100% {
    opacity: 1;
    transform: translateY(0);
  }
}

.animate-fade-in {
  animation: fade-in 0.8s ease-out forwards;
  opacity: 0;
}

.delay-150 {
  animation-delay: 0.15s;
}

.delay-300 {
  animation-delay: 0.3s;
}
</style>
