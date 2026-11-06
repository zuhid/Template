<!-------------------------------------------------- script -------------------------------------------------->
<script setup>
import { useRouter } from 'vue-router';
import { accountStore } from '../store';

const router = useRouter();

function logout() {
  accountStore.logout();
  router.push('/login');
}
</script>

<!-------------------------------------------------- template -------------------------------------------------->
<template>
  <nav class="navbar">
    <span class="navbar-brand">Template</span>
    <div class="navbar-links">
      <router-link to="/home">Home</router-link>
      <router-link to="/string-types">String Types</router-link>
      <router-link to="/date-types">Date Types</router-link>
      <router-link to="/numeric-types">Numeric Types</router-link>
    </div>
    <Button id="logout" label="Logout" @click="logout" />
  </nav>
  <router-view />
</template>

<!-------------------------------------------------- style -------------------------------------------------->
<style scoped>
.navbar {
  display: flex;
  align-items: center;
  background-color: var(--primary-color);
  color: var(--primary-color-text);
  padding: 0 1.5rem;
  height: 54px;
  box-shadow: var(--shadow-popup);
  position: sticky;
  top: 0;
  z-index: 100;
  gap: 0.75rem;
}

.navbar-brand {
  font-size: 1.15rem;
  font-weight: var(--font-weight-bold);
  letter-spacing: 0.04em;
  margin-right: 1rem;
}

.navbar-links {
  display: flex;
  gap: 0.25rem;
  flex: 1;
}

.navbar-links a {
  color: var(--on-primary-muted);
  text-decoration: none;
  padding: 0.35rem 0.8rem;
  border-radius: var(--radius);
  font-size: 0.9rem;
}

.navbar-links a:hover,
.navbar-links a.router-link-exact-active {
  background-color: var(--on-primary-hover);
  color: var(--primary-color-text);
}
</style>
