﻿using System;
using System.Collections.Generic;

namespace CH.CleanArchitecture.Common
{
    public static class ResultExtensions
    {
        public static Result WithMessage(this Result result, string message) {
            result.Message = message;
            return result;
        }

        public static Result WithError(this Result result, string error) {
            result.AddError(error);
            return result;
        }

        public static Result WithError(this Result result, string error, string code) {
            result.AddError(error, code);
            return result;
        }

        public static Result WithErrors(this Result result, List<IResultError> errors) {
            result.AddErrors(errors);
            return result;
        }

        public static Result WithErrors(this Result result, List<ResultError> errors) {
            result.AddErrors(errors);
            return result;
        }

        /// <summary>
        /// Marks the <paramref name="result"/> as failed
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Result Fail(this Result result) {
            result.IsSuccessful = false;
            return result;
        }

        /// <summary>
        /// Marks the <paramref name="result"/> as successful
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Result Succeed(this Result result) {
            result.IsSuccessful = true;
            return result;
        }

        public static Result WithException(this Result result, Exception exception) {
            result.Exception = exception;
            return result;
        }

        public static Result<T> WithMessage<T>(this Result<T> result, string message) {
            result.Message = message;
            return result;
        }

        public static Result<T> WithData<T>(this Result<T> result, T data) {
            result.Data = data;
            return result;
        }

        public static Result<T> WithError<T>(this Result<T> result, string error) {
            result.AddError(error);
            return result;
        }

        public static Result<T> WithErrors<T>(this Result<T> result, List<ResultError> errors) {
            result.AddErrors(errors);
            return result;
        }

        public static Result<T> WithError<T>(this Result<T> result, string error, string code) {
            result.AddError(error, code);
            return result;
        }

        /// <summary>
        /// Marks the <paramref name="result"/> as failed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Result<T> Fail<T>(this Result<T> result) {
            result.IsSuccessful = false;
            return result;
        }

        /// <summary>
        /// Marks the <paramref name="result"/> as successful
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <returns></returns>
        public static Result<T> Succeed<T>(this Result<T> result) {
            result.IsSuccessful = true;
            return result;
        }

        public static Result<T> WithException<T>(this Result<T> result, Exception exception) {
            result.Exception = exception;
            return result;
        }

        public static T Unwrap<T>(this Result<T> result) {
            if (result == null) {
                return default;
            }

            return result.Data;
        }
    }
}
