import axios from "axios";

export const getDashboard = () =>
  axios.get("http://localhost:5072/api/dashboard");