<!-------------------------------------------------- script -------------------------------------------------->
<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { accountStore } from '../store';

const router = useRouter();
const model = ref({ email: "admin@test.com", password: "" });

async function login() {
  if (await accountStore.login(model.value)) {
    router.push('/');
  }
}
</script>

<!-------------------------------------------------- template -------------------------------------------------->
<template>
  <Card id="login" title="Login" saveText="Login" cancelText="" @onSave="login">
    <Text label="Email" v-model="model.email" />
    <Password label="Password" v-model="model.password" />
  </Card>
</template>

<!-------------------------------------------------- style -------------------------------------------------->
<style scoped>
#login {
  width: 400px;
}
</style>
