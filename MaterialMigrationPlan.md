# Material Entity Migration Plan

## Overview
This document outlines the plan for migrating from the legacy Material entity to the new BaseProduct/ProductVariant structure and eventual removal of Material.cs.

## Current Status
- ✅ Material entity updated to use UnitType/UnitValue instead of Unit
- ✅ MaterialDto updated for compatibility
- ✅ Material-related handlers and controllers updated
- ✅ Bulk migration functionality implemented
- ✅ Backward compatibility maintained with deprecation warnings
- ✅ Domain tests updated

## Migration Steps

### Phase 1: Data Migration (Completed)
1. **Single Material Migration**: Use `POST /api/materials/migrate` endpoint
2. **Bulk Material Migration**: Use `POST /api/materials/bulk-migrate` endpoint
3. **Migration Tracking**: Monitor migration results and handle failures

### Phase 2: Code Migration (In Progress)
1. **Update Client Applications**: Migrate API consumers to use BaseProduct/ProductVariant endpoints
2. **Update Internal Dependencies**: Replace Material references with BaseProduct/ProductVariant
3. **Update Tests**: Complete test migration (remaining test files need updates)

### Phase 3: Removal Preparation (Next Steps)
1. **Mark for Removal**: All Material-related classes marked with `[Obsolete]` attribute
2. **Create Removal Checklist**: Document all components that need updates
3. **Update Documentation**: Update API documentation to reflect new structure

## Files to Update Before Removal

### Test Files (High Priority)
- `FunnyActivities.Application.UnitTests/TestData/MaterialTestData.cs`
- `FunnyActivities.Application.UnitTests/Validators/MaterialManagement/UpdateMaterialCommandValidatorTests.cs`
- `FunnyActivities.Application.UnitTests/Validators/MaterialManagement/UpdateMaterialRequestValidatorTests.cs`
- `FunnyActivities.Application.UnitTests/Validators/MaterialManagement/GetMaterialsQueryValidatorTests.cs`
- `FunnyActivities.Application.UnitTests/UpdateMaterialCommandHandlerTests.cs`
- `FunnyActivities.Application.UnitTests/Handlers/MaterialManagement/UpdateMaterialCommandHandlerTests.cs`
- `FunnyActivities.Application.UnitTests/GetMaterialsQueryHandlerTests.cs`

### Application Layer
- Update all Material-related commands to use new structure
- Update all Material-related queries to use new structure
- Update all Material-related validators

### Infrastructure Layer
- Update repository interfaces and implementations
- Update database context and migrations
- Update any Material-specific database operations

### API Layer
- Consider adding migration status endpoints
- Update API documentation
- Add migration progress tracking

## Removal Checklist

### Before Removal
- [ ] All existing materials migrated to BaseProduct/ProductVariant
- [ ] All client applications updated to use new endpoints
- [ ] All internal dependencies updated
- [ ] All tests passing with new structure
- [ ] Database migration scripts created and tested
- [ ] API documentation updated

### During Removal
- [ ] Remove Material.cs entity file
- [ ] Remove Material-related DTOs
- [ ] Remove Material-related commands and handlers
- [ ] Remove Material-related controllers
- [ ] Remove Material-related repositories
- [ ] Update database context
- [ ] Remove Material-related database tables (if applicable)

### After Removal
- [ ] Update all references in documentation
- [ ] Update API versioning information
- [ ] Clean up any remaining migration code
- [ ] Update deployment scripts
- [ ] Notify stakeholders of successful migration

## Risk Assessment

### High Risk
- Data loss during migration
- Breaking changes for API consumers
- Database migration failures

### Medium Risk
- Test failures during transition
- Performance issues with bulk migration
- Incomplete migration of edge cases

### Low Risk
- Code cleanup after migration
- Documentation updates
- Minor API endpoint changes

## Rollback Plan
If issues arise during migration:
1. Stop migration process
2. Restore from backup if necessary
3. Revert code changes
4. Communicate with stakeholders
5. Analyze root cause and create improved migration plan

## Success Criteria
- All existing materials successfully migrated
- All tests passing
- All client applications functioning
- No data loss
- Performance maintained or improved
- API backward compatibility maintained during transition

## Timeline
- **Phase 1**: Data Migration - 1-2 weeks
- **Phase 2**: Code Migration - 2-4 weeks
- **Phase 3**: Removal - 1 week
- **Total**: 4-7 weeks

## Responsible Parties
- **Development Team**: Code updates and testing
- **DevOps Team**: Database migrations and deployment
- **QA Team**: Testing and validation
- **Product Team**: Stakeholder communication
- **Client Teams**: API consumer updates

## Monitoring and Reporting
- Daily migration progress reports
- Error rate monitoring
- Performance monitoring
- Stakeholder status updates
- Risk assessment updates

## Communication Plan
- Weekly status updates to stakeholders
- Migration progress dashboard
- Issue tracking and resolution updates
- Go-live announcement
- Post-migration support plan