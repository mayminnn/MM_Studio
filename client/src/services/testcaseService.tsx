import axios from "axios";

const API_URL = "http://localhost:5072/api/testcases";

export const getTestCases = async () => {
  const res = await axios.get(API_URL);
  return res.data;
};