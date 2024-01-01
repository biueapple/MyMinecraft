using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crafting : MonoBehaviour
{
    //recipes 는 일단 있는데 사용처는 없음 그냥 필요하면 쓸듯 (어느 조건에 해당한 조합법을 모아놓고 보여준다던가)
    List<Recipe> recipes = new List<Recipe>();
    //모든 경우에 따라 분류해놓은 레시피 (hashTable 느낌으로)
    List<Recipe> recipes3X3 = new List<Recipe>();
    List<Recipe> recipes3X2 = new List<Recipe>();
    List<Recipe> recipes2X3 = new List<Recipe>();
    List<Recipe> recipes2X2 = new List<Recipe>();
    List<Recipe> recipes3X1 = new List<Recipe>();
    List<Recipe> recipes1X3 = new List<Recipe>();
    List<Recipe> recipes2X1 = new List<Recipe>();
    List<Recipe> recipes1X2 = new List<Recipe>();
    List<Recipe> recipes1X1 = new List<Recipe>();

    private void Start()
    {
        //모든 조합법은 만들고 레시피에 add해야지만 나옴
        Recipe r = new Plank_Recipe();
        RecipeAdd(r);
        r = new WoodSkick_Recipe();
        RecipeAdd(r);
    }

    //레시피에 추가하는 함수 레시피를 만들고 넣주면 끝
    public void RecipeAdd(Recipe r)
    {
        if (r.recipe is Table3X3)
            recipes3X3.Add(r);
        else if (r.recipe is Table3X2)
            recipes3X2.Add(r);
        else if (r.recipe is Table2X3)
            recipes2X3.Add(r);
        else if (r.recipe is Table2X2)
            recipes2X2.Add(r);
        else if (r.recipe is Table3X1)
            recipes3X1.Add(r);
        else if (r.recipe is Table1X3)
            recipes1X3.Add(r);
        else if (r.recipe is Table2X1)
            recipes2X1.Add(r);
        else if (r.recipe is Table1X2)
            recipes1X2.Add(r);
        else if (r.recipe is Table1X1)
            recipes1X1.Add(r);
    }

    //조합법을 확인하는 함수
    public void Combination(Table table, out int id, out int result)
    {
        id = -1;
        result = 0;
        if (table.codes == null)
            return;
        if (table is Table3X3)
        {
            for (int i = 0; i < recipes3X3.Count; i++)
            {
                if (recipes3X3[i].Comparison(table))
                {
                    id = recipes3X3[i].result;
                    result = recipes3X3[i].resultCount;
                    return;
                }
            }
        }
        else if (table is Table3X2)
        {
            for (int i = 0; i < recipes3X2.Count; i++)
            {
                if (recipes3X2[i].Comparison(table))
                {
                    id = recipes3X2[i].result;
                    result = recipes3X2[i].resultCount;
                    return;
                }
            }
        }
        else if (table is Table2X3)
        {
            for (int i = 0; i < recipes2X3.Count; i++)
            {
                if (recipes2X3[i].Comparison(table))
                {
                    id = recipes2X3[i].result;
                    result = recipes2X3[i].resultCount;
                    return;
                }
            }
        }
        else if (table is Table2X2)
        {
            for (int i = 0; i < recipes2X2.Count; i++)
            {
                if (recipes2X2[i].Comparison(table))
                {
                    id = recipes2X2[i].result;
                    result = recipes2X2[i].resultCount;
                    return;
                }
            }
        }
        else if (table is Table3X1)
        {
            for (int i = 0; i < recipes3X1.Count; i++)
            {
                if (recipes3X1[i].Comparison(table))
                {
                    id = recipes3X1[i].result;
                    result = recipes3X1[i].resultCount;
                    return;
                }
            }
        }
        else if (table is Table2X2)
        {
            for (int i = 0; i < recipes1X3.Count; i++)
            {
                if (recipes1X3[i].Comparison(table))
                {
                    id = recipes1X3[i].result;
                    result = recipes1X3[i].resultCount;
                    return;
                }
            }
        }
        else if (table is Table2X1)
        {
            for (int i = 0; i < recipes2X1.Count; i++)
            {
                if (recipes2X1[i].Comparison(table))
                {
                    id = recipes2X1[i].result;
                    result = recipes2X1[i].resultCount;
                    return;
                }
            }
        }
        else if (table is Table1X2)
        {
            for (int i = 0; i < recipes1X2.Count; i++)
            {
                if (recipes1X2[i].Comparison(table))
                {
                    id = recipes1X2[i].result;
                    result = recipes1X2[i].resultCount;
                    return;
                }
            }
        }
        else if (table is Table1X1)
        {
            for (int i = 0; i < recipes1X1.Count; i++)
            {
                if (recipes1X1[i].Comparison(table))
                {
                    id = recipes1X1[i].result;
                    result = recipes1X1[i].resultCount;
                    return;
                }
            }
        }
    }
}

//기본적인 레시피 틀
public class Recipe
{
    //레시피가 들어가있는 공간
    public Table recipe;
    //어떤 아이템이 나오는지 byte가 아니라 int인 이유는 byte는 블록인덱스 int는 아이템 인덱스
    public int result;
    //아이템이 몇개가 나오는지
    public int resultCount;

    //레시피가 같은지 비교함
    public bool Comparison(Table x3)
    {
        return recipe._Equals(x3);
    }
}

//레시피를 작성하는 방법
public class Plank_Recipe : Recipe
{
    public Plank_Recipe()
    {
        //나올 아이템의 인덱스
        result = 6;
        //나올 아이템의 갯수
        resultCount = 4;

        //레시피 작성
        recipe = new Table3X3();
        recipe.codes = new int[3,3]
        {
            { 5, -1, -1 },
            { -1, -1, -1 },
            { -1,-1,-1 }
        };
        //레시피 압축
        recipe.Slice(recipe, out recipe);
    }
}

//레시피를 작성하는 방법
public class WoodSkick_Recipe : Recipe
{
    public WoodSkick_Recipe()
    {
        //나올 아이템의 인덱스
        result = 7;
        //나올 아이템의 갯수
        resultCount = 4;

        //레시피 작성
        recipe = new Table3X3();
        recipe.codes = new int[3, 3]
        {
            { 6, -1, -1 },
            { 6, -1, -1 },
            { -1,-1,-1 }
        };
        //레시피 압축
        recipe.Slice(recipe, out recipe);
    }
}

//테이블들
public class Table
{
    public int[,] codes;

    //code가 같은지 비교
    public bool _Equals(Table table)
    {
        if (table.codes.GetLength(0) != codes.GetLength(0) || table.codes.GetLength(1) != codes.GetLength(1))
            return false;

        for (int i = 0; i < table.codes.GetLength(0); i++)
        {
            for (int j = 0; j < table.codes.GetLength(1); j++)
            {
                if (table.codes[i, j] != codes[i, j])
                    return false;
            }
        }

        return true;
    }

    //테이블을 압축할때 새로운 테이블을 리턴해주는 함수
    public Table NewTable(int x, int y)
    {
        if (x == 3 && y == 3) return new Table3X3();
        else if (x == 3 && y == 2) return new Table3X2();
        else if (x == 2 && y == 3) return new Table2X3();
        else if (x == 3 && y == 1) return new Table3X1();
        else if (x == 1 && y == 3) return new Table1X3();

        else if (x == 2 && y == 2) return new Table2X2();
        else if (x == 2 && y == 1) return new Table2X1();
        else if (x == 1 && y == 2) return new Table1X2();

        else if (x == 1 && y == 1) return new Table1X1();

        return new Table();
    }

    //테이블을 압축해주는 함수 (table은 기준이 되는 기존의 테이블 slice는 압축된 테이블을 넣어줌)
    public void Slice(Table table, out Table slice)
    {
        //가장 앞 행부터
        int rowfront = 0;
        //가장 뒤 행까지
        int rowback = table.codes.GetLength(0);
        //가장 앞 열부터
        int linefront = 0;
        //가장 뒤 열까지
        int lineback = table.codes.GetLength(1);

        //앞에 -1이 아닌 code를 찾았는지
        bool forward = false;
        //뒤에서 -1이 아닌 code를 찾았는지
        bool reverse = false;

        //행에서 앞에서부터 -1이 아닌 code를 찾을때까지 
        for (int i = 0; i < table.codes.GetLength(0); i++)
        {
            rowfront = i;
            for (int j = 0; j < table.codes.GetLength(1); j++)
            {
                if (table.codes[i, j] != -1)
                {
                    forward = true;
                    break;
                }
            }
            if (forward)
                break;
        }

        //모든칸이 -1인경우 아무것도 들어있지 않은 테이블이니 그냥 나가기
        if(rowfront == table.codes.GetLength(0) - 1 && forward == false)
        {
            slice = new Table();
            return;
        }

        //행에서 뒤에서부터 -1이 아닌 code를 찾을때까지
        for (int i = table.codes.GetLength(0) - 1; i >= 0; i--)
        {
            rowback = i;
            for (int j = 0; j < table.codes.GetLength(1); j++)
            {
                if (table.codes[i, j] != -1)
                {
                    reverse = true;
                    break;
                }
            }
            if (reverse)
                break;
        }

        //rowfront 가 시작줄 rowback 이 마지막 줄 rowfront ~ rowback 가 행의 크기

        forward = false; reverse = false;

        //열도 행과 마찬가지의 루틴
        for (int i = 0; i < table.codes.GetLength(1); i++)
        {
            linefront = i;
            for (int j = 0; j < table.codes.GetLength(0); j++)
            {
                if (table.codes[j, i] != -1)
                {
                    forward = true;
                    break;
                }
            }
            if (forward)
                break;
        }

        for (int i = table.codes.GetLength(1) - 1; i >= 0; i--)
        {
            lineback = i;
            for (int j = 0; j < table.codes.GetLength(0); j++)
            {
                if (table.codes[j, i] != -1)
                {
                    reverse = true;
                    break;
                }
            }
            if (reverse)
                break;
        }

        //행과 열 둘다 어디부터 어디까지가 필요한 열과 행인지를 알아냈으니
        //그 크기를 가지고
        int line = Mathf.Abs(linefront - lineback) + 1;
        int row  = Mathf.Abs(rowfront - rowback) + 1;
        //테이블을 만들어서
        slice = NewTable(row, line);
        //code를 채워놓기
        for(int i = rowfront; i <= rowback; i++)
        {
            for(int j = linefront; j <= lineback; j++)
            {
                slice.codes[i - rowfront, j - linefront] = table.codes[i, j];
            }
        }
    }
}

public class Table3X3 : Table
{
    public Table3X3()
    {
        codes = new int[3, 3]
        {
            { -1, -1, -1},
            { -1, -1, -1},
            { -1, -1, -1}
        };
    }
}

public class Table2X3 : Table
{
    public Table2X3()
    {
        codes = new int[2, 3]
        {
            { -1, -1, -1},
            { -1, -1, -1}
        };
    }
}

public class Table3X2 : Table
{
    public Table3X2()
    {
        codes = new int[3, 2]
        {
            { -1, -1},
            { -1, -1},
            { -1, -1}
        };
    }
}

public class Table2X2 : Table
{
    public Table2X2()
    {
        codes = new int[2, 2]
        {
            { -1, -1},
            { -1, -1},
        };
    }
}

public class Table3X1 : Table
{
    public Table3X1()
    {
        codes = new int[3, 1]
        {
            { -1 },
            { -1 },
            { -1 }
        };
    }
}

public class Table1X3 : Table
{
    public Table1X3()
    {
        codes = new int[1, 3]
        {
            {-1,-1, -1}
        };
    }
}

public class Table1X2 : Table
{
    public Table1X2()
    {
        codes = new int[1, 2]
        {
            {-1,-1 }
        };
    }
}

public class Table2X1 : Table
{
    public Table2X1()
    {
        codes = new int[2, 1]
        {
            { -1},
            {-1 }
        };
    }
}

public class Table1X1 : Table
{
    public Table1X1()
    {
        codes = new int[1, 1] 
        { { -1 } };
    }
}