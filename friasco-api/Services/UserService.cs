﻿using AutoMapper;
using friasco_api.Data.Entities;
using friasco_api.Data.Repositories;
using friasco_api.Helpers;
using friasco_api.Models;

namespace friasco_api.Services;

public interface IUserService
{
    Task<IEnumerable<User>> GetAll();
    Task<User> GetById(int id);
    Task<int> Create(UserCreateRequestModel model);
    Task<int> Update(int id, UserUpdateRequestModel model);
    Task<int> Delete(int id);
}

public class UserService : IUserService
{
    private readonly ILogger<IUserService> _logger;
    private readonly IMapper _mapper;
    private IUserRepository _userRepository;

    public UserService(ILogger<IUserService> logger, IMapper mapper, IUserRepository userRepository)
    {
        _logger = logger;
        _mapper = mapper;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<User>> GetAll()
    {
        _logger.LogDebug("UserService::GetAll");
        return await _userRepository.GetAll();
    }

    public async Task<User> GetById(int id)
    {
        _logger.LogDebug($"UserService::GetById id: {id}");

        var user = await _userRepository.GetById(id);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with id: {id} not found");
        }

        return user;
    }

    public async Task<int> Create(UserCreateRequestModel model)
    {
        _logger.Log(LogLevel.Debug, "UserService::Create");

        if (await _userRepository.GetByEmail(model.Email!) != null)
        {
            throw new CustomAppException($"User with the email: {model.Email} already exists");
        }

        var user = _mapper.Map<User>(model);

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

        var rowsAffectedResult = await _userRepository.Create(user);

        return rowsAffectedResult;
    }

    public async Task<int> Update(int id, UserUpdateRequestModel model)
    {
        _logger.LogDebug($"UserService::Update id: {id}");

        var user = await _userRepository.GetById(id);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with id: {id} not found");
        }

        if (!string.IsNullOrEmpty(model.Email))
        {
            // Check if new email already exists
            if ((model.Email != user.Email) && await _userRepository.GetByEmail(model.Email) != null)
            {
                throw new CustomAppException($"User with the email: {model.Email} already exists");
            }

        }

        if (model.Role == null) {
            // Stop role defaulting to 0 on an empty request
            model.Role = user.Role;
        }

        if (!string.IsNullOrEmpty(model.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        }

        _mapper.Map(model, user);

        var rowsAffectedResult = await _userRepository.Update(user);

        return rowsAffectedResult;
    }

    public async Task<int> Delete(int id)
    {
        _logger.LogDebug($"UserService::Delete id: {id}");

        var user = await _userRepository.GetById(id);

        if (user == null)
        {
            throw new KeyNotFoundException($"User with id: {id} not found");
        }

        var rowsAffectedResult = await _userRepository.Delete(id);

        return rowsAffectedResult;
    }
}
