import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import pinia from "./stores";

import ElementPlus from "element-plus";
import "element-plus/dist/index.css";

import "./assets/main.css";
import "@fortawesome/fontawesome-free/css/all.min.css";

import { mask } from "vue-the-mask";
import "leaflet/dist/leaflet.css";
import "maplibre-gl/dist/maplibre-gl.css";

const app = createApp(App);
app.use(pinia);
app.use(router);
app.use(ElementPlus);
app.directive("mask", mask);

app.mount("#app");
