import axios from "axios";

const API = "http://localhost:5072/api/suite";

export const getSuites = async () => {
    const res = await axios.get(API);
    return res.data;
};

export const createSuite = async (suite: any) => {
    const res = await axios.post(API, suite);
    return res.data;
};