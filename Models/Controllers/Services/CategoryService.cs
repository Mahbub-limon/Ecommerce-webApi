using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Ecommerce_webApi.DTOs;
using Ecommerce_webApi.Enums;
using Ecommerce_webApi.Helpers;
using Ecommerce_webApi.Models.Controllers.Interfaces;
using Ecommerce_webApi.Models.data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_webApi.Models.Controllers.Services
{
    //controller => Service => database
    public class CategoryService : ICategoryService 
    {
        //  private static readonly List<Category> _categories = new List<Category>();  //like as an object for Category model  //inmemory data  

        private readonly AppDbContext _appDbContext; //appDbContext is a variable and wich type is AppDbContext

        private readonly IMapper _mapper;  //Here _mapper is the variable of IMapper

        public CategoryService(AppDbContext appDbContext,IMapper mapper) //Dependence injection
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        //Context (Application -> Context -> Database)
         public async Task<PaginatedResult<CategoryReadDto>> GetAllCategories(QueryParameters queryParameters)   //all category replayed
         { 
            //Decompose the queryParameters into local variable
            var pageNumber = queryParameters.pageNumber; 
            var pageSize = queryParameters.PageSize;
            var search = queryParameters.Search;
            var sortOrder = queryParameters.sortOrder;

            IQueryable<Category> query = _appDbContext.Categories;

            //search by name or description
            if(!string.IsNullOrWhiteSpace(search))
            { 
                var formattedSearch = $"%{search.Trim()}%";
             query = query.Where(c => EF.Functions.Like(c.Name,formattedSearch) || EF.Functions.Like(c.Description,formattedSearch));
            }

            //sorting
            if(string.IsNullOrWhiteSpace(sortOrder))
            {
                query = query.OrderBy(c => c.Name);
            }
            else
            {
                var formattedSortOrder = sortOrder.Trim().ToLower();

                if (Enum.TryParse<SortOrder>(formattedSortOrder,true,out var parsedSortOrder))
                {
                query = parsedSortOrder switch
                {
                    SortOrder.NameAsc => query.OrderBy( c => c.Name),
                    SortOrder.NameDesc => query.OrderByDescending( c => c.Name),
                    SortOrder.CreateAtAsc => query.OrderBy(c => c.CreateAdt),
                    SortOrder.createdAtDesc => query.OrderByDescending( c => c.CreateAdt),
                    _ => query.OrderBy(c => c.Name),     //Default sorting for invalid sortOrder
                };
                
            }
            }
            //get total count
            var totalCount = await query.CountAsync();

            //pagination,pageNumber = 3 ,pageSize = 5
            //20 categories
            //skip ((pageNumber -1)* pageSize).Take(pageSize)
            var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            var results = _mapper.Map<List<CategoryReadDto>>(items);  //Converting _categories to CategoryRedDTO
            
            return new PaginatedResult<CategoryReadDto>
            {
                Items = results,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                pageSize = pageSize
            };
        }

         public async Task<CategoryReadDto?> GetCategoryById(Guid categoryId)
         {
            var foundCategory =await  _appDbContext.Categories.FindAsync(categoryId);

        return foundCategory == null ? null : _mapper.Map<CategoryReadDto>(foundCategory);
            
        }

    public async Task<CategoryReadDto> CreateCategory (CategoryCrieateDto categoryData)
    {
        //CategoryCreateDto  => Category

        var newCategory =_mapper.Map<Category>(categoryData);
        await _appDbContext.Categories.AddAsync(newCategory);  //datga ready for inserting table
        await _appDbContext.SaveChangesAsync(); // finally save in the table
        return _mapper.Map<CategoryReadDto>(newCategory);

    }
    public async Task<CategoryReadDto ?> UpdateCategoryById(Guid categoryId, CategoryUpdateDto categoryData)
    {
        var foundCategory = await _appDbContext.Categories.FindAsync(categoryId);

        if(foundCategory == null)
        {
            return null;
        }
            
            //CategoryUpdateDto => Category
             _mapper.Map(categoryData,foundCategory); //Convertting categoryData to foundCategory
             _appDbContext.Categories.Update(foundCategory);
            await _appDbContext.SaveChangesAsync();
            return _mapper.Map<CategoryReadDto>(foundCategory);
    }

            public async Task<bool> DeleteCategoryById(Guid categoryId)
            {
                var foundCategory = await _appDbContext.Categories.FindAsync(categoryId);

                if(foundCategory == null)
                {
                    return false;
                }

                _appDbContext.Categories.Remove(foundCategory);
                await _appDbContext.SaveChangesAsync();
                return true;
            }
    }
}         
