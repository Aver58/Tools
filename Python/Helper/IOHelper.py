import pandas as pd
import numpy as np


def Is_File_Empty(fileName):
    with open(fileName, "rb") as f:
        f.seek(0, 2) # EOF
        size = f.tell()
        f.seek(0, 0) # BOF
    if size == 0:
        print(fileName, " is empty")
    return size == 0


# 方便读写的类
class IOHelper(object):
    @staticmethod
    def Write(file_name, data='', mode='a'):
        with open(file_name, mode) as f:
            f.write(data)

    @staticmethod
    def Write_CSV_With_Pandas(data, pathName, encoding=None):
        data.to_csv(pathName, encoding=encoding)

    @staticmethod
    def Write_XLS_With_Pandas(data, pathName, encoding=None):
        data.to_excel(pathName, encoding=encoding)

    @staticmethod
    def Write_CSV_With_Numpy(my_matrix, pathName , fmt='%.18e', encoding=None):
        np.savetxt(pathName, my_matrix, fmt=fmt, encoding=encoding)

    @staticmethod
    def Read_CSV_With_Pandas(fileName):
        return pd.read_csv(fileName, encoding='ISO-8859-1')  # UTF-8解码不来

    @staticmethod
    def Read_CSV_With_Numpy(fileName, dtype=float):
        return np.loadtxt(fileName, dtype=dtype, delimiter=",")

    @staticmethod
    def Read_CSV_To_Array(fileName):
        csv_data = pd.read_csv(fileName, encoding='UTF-8')
        # csv_data是数据类型是‘list',转化为数组类型好处理
        csv_data = np.array(csv_data)
        print("csv_data.shape ：", csv_data.shape)
        return csv_data

    @staticmethod
    def Read_XLS_To_Array(fileName):
        if Is_File_Empty(fileName):
            return []

        xls_data = pd.read_excel(fileName, encoding='UTF-8')
        print(fileName, "shape ：", xls_data.shape)
        newArray = np.array(xls_data)
        return newArray

    # 读取Xls文件==》从0开始的List
    @staticmethod
    def Read_XLS_To_List(fileName):
        if Is_File_Empty(fileName):
            return {}

        xls_data = pd.read_excel(fileName, encoding='UTF-8')
        print(fileName, "shape ：", xls_data.shape)

        column = xls_data.columns
        newDic = {}
        index = 0
        for info in xls_data.values:
            for i in range(len(info)):
                if not newDic.get(index):
                    newDic[index] = {}
                newDic[index][column[i]] = info[i]

            index = index + 1

        return newDic

    @staticmethod
    def GetShape(fileName):
        # todo 不是xls要return
        if Is_File_Empty(fileName):
            return 0
        xls_data = pd.read_excel(fileName, encoding='UTF-8')
        return xls_data.shape[0]
