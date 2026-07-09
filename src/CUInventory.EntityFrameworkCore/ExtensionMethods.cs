using System;
using System.Linq.Expressions;
using CUInventory.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CUInventory;


public static class ModuleTableExtensions
{
    public static EntityTypeBuilder<TEntity> ToModuleTable<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        Expression<Func<CUInventoryDbContext, DbSet<TEntity>>> dbSetSelector)
        where TEntity : class
    {
        return entityTypeBuilder.ToTable(CUInventoryConsts.DbTablePrefix + GetDbSetName(dbSetSelector), CUInventoryConsts.DbSchema);
    }


    public static EntityTypeBuilder<TEntity> ToModuleTable<TEntity>(
        this EntityTypeBuilder<TEntity> entityTypeBuilder,
        string tableName)
        where TEntity : class
    {
        return entityTypeBuilder.ToTable(CUInventoryConsts.DbTablePrefix + tableName, CUInventoryConsts.DbSchema);
    }


    public static OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToModuleTable<TOwnerEntity, TDependentEntity>(
        this OwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ownedNavigationBuilder,
        string tableName)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        return ownedNavigationBuilder.ToTable(CUInventoryConsts.DbTablePrefix + tableName, CUInventoryConsts.DbSchema);
    }

    private static string GetDbSetName<TEntity>(
        Expression<Func<CUInventoryDbContext, DbSet<TEntity>>> dbSetSelector)
        where TEntity : class
    {
        if (dbSetSelector.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        throw new ArgumentException(
            "The DbSet selector must be a direct member access on the DbContext, e.g. x => x.Products.",
            nameof(dbSetSelector)
        );
    }

}
