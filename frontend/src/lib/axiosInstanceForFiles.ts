import axios from "axios";
import Cookies from "js-cookie";

const axiosInstanceForFiles = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  headers: {
    "Content-Type": "multipart/form-data",
  },
});

axiosInstanceForFiles.interceptors.request.use(
  (config) => {
    const token = Cookies.get("oasisToken");
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export default axiosInstanceForFiles;
